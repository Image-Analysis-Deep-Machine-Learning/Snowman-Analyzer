import os
import cv2
import torch
import argparse
import numpy as np
from tqdm import tqdm
from collections import deque
import xml.etree.ElementTree as ET
from ultralytics import YOLO, RTDETR

# Dictionary to store history of centers for speed calculation
track_history = {}

# Create the root and metadata for output xml
root = ET.Element("data")
metadata = ET.SubElement(root, "metadata")
ET.SubElement(metadata, "data_id").text = "sa_dataset"
ET.SubElement(metadata, "parent").text = ""
ET.SubElement(metadata, "version_major").text = "2"
ET.SubElement(metadata, "xml_sid").text = ""
ET.SubElement(metadata, "description").text = "Anotacny nastroj v1.4.1"

# Container for all frames
images_node = ET.SubElement(root, "images")


def calculate_speed(track_id, current_center, fps, ppm):
    if track_id not in track_history:
        track_history[track_id] = deque(maxlen=10)
        track_history[track_id].append(current_center)
        return 0.0

    prev_center = track_history[track_id][0]
    track_history[track_id].append(current_center)

    pixel_distance = np.linalg.norm(np.array(current_center) - np.array(prev_center))
    num_frames = len(track_history[track_id]) - 1

    # Calculate time_delta based on the effective FPS (Target FPS)
    time_delta = num_frames / fps

    if time_delta == 0: return 0.0

    speed_px_per_sec = pixel_distance / time_delta

    if ppm > 0:
        return (speed_px_per_sec / ppm) * 3.6

    return speed_px_per_sec


def process_video(args):
    # 1. Device selection
    device = '0' if args.gpu_accelerated and torch.cuda.is_available() else 'cpu'

    # 2. Model Selection
    if "RTDETR" in args.detector.upper():
        print(f"📡 Initializing RT-DETR engine...")
        model = RTDETR(args.weights)
    else:
        print(f"📡 Initializing {args.detector} engine...")
        model = YOLO(args.weights)
    print(f"🚀 Detector: {args.detector} | Tracker: {args.tracker} | Speed Est: ON")

    cap = cv2.VideoCapture(args.input_video)

    # Input Video Properties
    original_fps = cap.get(cv2.CAP_PROP_FPS)
    total_input_frames = int(cap.get(cv2.CAP_PROP_FRAME_COUNT))
    original_w = int(cap.get(cv2.CAP_PROP_FRAME_WIDTH))
    original_h = int(cap.get(cv2.CAP_PROP_FRAME_HEIGHT))

    # Determine Target FPS and Resolution
    # If user didn't specify target fps, use original.
    target_fps = args.target_fps if args.target_fps is not None else original_fps

    # If user didn't specify resolution, use original.
    target_w = args.target_width if args.target_width is not None else original_w
    target_h = args.target_height if args.target_height is not None else original_h

    # Calculate frame stride (skip interval)
    # e.g., 60fps input / 15fps target = 4. Process every 4th frame.
    frame_stride = max(1, int(round(original_fps / target_fps)))

    print(f"ℹ️  Input: {original_w}x{original_h} @ {original_fps:.2f} FPS")
    print(f"🎯 Target: {target_w}x{target_h} @ {target_fps:.2f} FPS (Stride: {frame_stride})")

    # Output Setup
    # Create 'frames' subdirectory
    frames_dir = os.path.join(args.output_dir, "frames")
    if not os.path.exists(frames_dir):
        os.makedirs(frames_dir)

    video_writer = None
    if args.make_video:
        # Note: The output video will be at target_fps and target_resolution
        out_path = os.path.join(args.output_dir, "annotated_output.mp4")
        video_writer = cv2.VideoWriter(out_path, cv2.VideoWriter_fourcc(*'mp4v'), target_fps, (target_w, target_h))

    # Progress Bar
    pbar = tqdm(total=total_input_frames, desc="🎥 Processing Video", unit="frame")

    frame_idx = 0

    while True:
        ret, frame = cap.read()
        if not ret: break

        # Update pbar for every frame read
        pbar.update(1)
        frame_idx += 1

        # Frame Skipping Logic
        # Only process if frame index matches the stride
        # We subtract 1 from frame_idx for modulo because frame_idx starts at 1 in this loop logic
        if (frame_idx - 1) % frame_stride != 0:
            continue

        # Resize Frame
        # We must resize BEFORE detection so bounding boxes are correct for the saved image
        if target_w != original_w or target_h != original_h:
            frame = cv2.resize(frame, (target_w, target_h))

        # Detection & Tracking
        if "RTDETR" in args.detector.upper():
            results = model.track(
                source=frame,
                persist=True,
                conf=0.3,
                tracker=args.tracker + ".yaml",
                device=device,
                verbose=False
            )
        elif "YOLO" in args.detector.upper():
            results = model.track(
                source=frame,
                persist=True,
                tracker=args.tracker + ".yaml",
                classes=args.coco_classes,
                device=device,
                verbose=False
            )

        # Save Frame to Disk
        # "frame_00000.jpeg" using the ORIGINAL frame index
        frame_filename = f"frame_{frame_idx:05}.jpeg"
        cv2.imwrite(os.path.join(frames_dir, frame_filename), frame)

        # XML Logging
        image_node = ET.SubElement(images_node, "image")
        ET.SubElement(image_node, "src").text = frame_filename
        bboxes_node = ET.SubElement(image_node, "boundingboxes")

        if results[0].boxes.id is not None:
            boxes = results[0].boxes.xyxy.cpu().numpy()
            ids = results[0].boxes.id.cpu().numpy().astype(int)
            clss = results[0].boxes.cls.cpu().numpy().astype(int)

            for box, obj_id, cls in zip(boxes, ids, clss):
                x1, y1, x2, y2 = box
                center = ((x1 + x2) / 2, (y1 + y2) / 2)

                # Estimate speed using TARGET FPS
                # Because the time difference between this frame and the previous *processed* frame
                # corresponds to 1/target_fps seconds.
                speed = calculate_speed(obj_id, center, target_fps, args.ppm)
                unit = "km/h" if args.ppm > 0 else "px/s"

                bbox = ET.SubElement(bboxes_node, "boundingbox")
                ET.SubElement(bbox, "x_left_top").text = str(int(x1))
                ET.SubElement(bbox, "y_left_top").text = str(int(y1))
                ET.SubElement(bbox, "width").text = str(int(x2 - x1))
                ET.SubElement(bbox, "height").text = str(int(y2 - y1))

                cls_name = ET.SubElement(bbox, "class_name")
                ET.SubElement(cls_name, "project_id").text = str(model.names[cls])
                ET.SubElement(cls_name, "track_id").text = str(obj_id)

                if args.make_video:
                    # Setting colors
                    bbox_color = (255, 0, 157) # vibrant pirple
                    text_bg_color = (255, 0, 157)
                    text_color = (255, 255, 255) # pure white

                    label = f"ID:{obj_id} {model.names[cls]} {speed:.1f} {unit}"

                    # Getting text size for the background box
                    (text_w, text_h), baseline = cv2.getTextSize(label, cv2.FONT_HERSHEY_SIMPLEX, 0.5, 1)

                    # Drawing bbox
                    cv2.rectangle(
                        frame,
                        (int(x1),
                         int(y1)),
                        (int(x2),
                         int(y2)),
                        bbox_color,
                        2
                    ) # (225, 21, 62) old color

                    # Drawing filled label background
                    cv2.rectangle(
                        frame,
                        (int(x1), int(y1) - text_h - 10),
                        (int(x1) + text_w, int(y1)),
                        text_bg_color,
                        -1 # fills the rectangle fully
                    )

                    # Drawing text on top of the background
                    cv2.putText(
                        frame,
                        label,
                        (int(x1), int(y1 - 10)),
                        cv2.FONT_HERSHEY_SIMPLEX,
                        0.5,
                        text_color, # (216, 105, 126) old color
                        2
                    )

        if video_writer: video_writer.write(frame)

    # 5. Save XML data & clean up
    pbar.close()
    cap.release()
    if video_writer: video_writer.release()

    video_filename = os.path.basename(args.input_video)
    video_name_only = os.path.splitext(video_filename)[0]

    # Save XML inside the frames folder as requested?
    # Or in the output_dir root? User said: "output_dir/frames/input_video_name_metadata.xml"
    # Let's save it exactly where requested: inside frames dir.
    xml_filename = f"_{video_name_only}_metadata.xml"
    output_xml_path = os.path.join(frames_dir, xml_filename)

    print(f"💾 Exporting tracking metadata to: {output_xml_path}")
    ET.indent(root, space="    ")
    ET.ElementTree(root).write(output_xml_path, encoding="utf-8", xml_declaration=True)

    print(f"✅ Finished.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--input_video", type=str, required=True)
    parser.add_argument("--detector", type=str, default="YOLOv26")
    parser.add_argument("--tracker", type=str, default="botsort", choices=["botsort", "bytetrack"])
    parser.add_argument("--weights", type=str, default="yolo26n.pt")
    parser.add_argument("--coco_classes", type=int, nargs='+', default=[2, 3, 5, 7])
    parser.add_argument("--gpu_accelerated", action="store_true")
    parser.add_argument("--output_dir", type=str, default="output")
    parser.add_argument("--make_video", action="store_true")
    parser.add_argument("--ppm", type=float, default=0, help="Pixels Per Meter.")
    parser.add_argument("--target_fps", type=float, default=None,
                        help="Target FPS for output (e.g., 15). Default: Keep original.")
    parser.add_argument("--target_width", type=int, default=None,
                        help="Target width (e.g., 1920). Default: Keep original.")
    parser.add_argument("--target_height", type=int, default=None,
                        help="Target height (e.g., 1080). Default: Keep original.")

    process_video(parser.parse_args())
    