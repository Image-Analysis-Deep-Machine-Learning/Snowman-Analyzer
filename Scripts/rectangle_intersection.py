# PROPERTIES_START
# INPUT_TYPE: EntityRectangle
# NAME: Rectangle Frame-wise Anomalies
# PROPERTIES_END

import matplotlib.pyplot as plt
import matplotlib
import numpy as np
from Snowman.Data import EventData
from System.Collections.Generic import List

matplotlib.use('qt5agg')

trackId_intersections = {}
trackId_firstOccurrences = {}

combined_string = "\n\n"
image_frame_index = 0

for image_frame in images_metadata:
    for bounding_box in image_frame.BoundingBoxes.BoundingBoxList:
        entity_intersect = bounding_box.XLeftTop < entity.Rectangle.Right and entity.Rectangle.X < bounding_box.XLeftTop + bounding_box.Width and bounding_box.YLeftTop < entity.Rectangle.Bottom and entity.Rectangle.Y < bounding_box.YLeftTop + bounding_box.Height

        if entity_intersect:
            id = bounding_box.ClassName.TrackId
            if id not in trackId_intersections:
                trackId_intersections[id] = 1
                trackId_firstOccurrences[id] = image_frame_index
            else:
                trackId_intersections[bounding_box.ClassName.TrackId] += 1

    image_frame_index += 1

i = 1
for key, value in trackId_intersections.items():
    combined_string += f"#{i}  ->  Track ID:{key}, IoNF: {value}\n"  # Intersections over Number of Frames
    i += 1


#sorted_dict = dict(sorted(trackId_intersections.items()))

trackIds = list([str(id) for id in trackId_intersections.keys()])
# use this in case you want them sorted, uncomment sorted_dict
#trackIds = list([str(id) for id in sorted_dict.keys()])

intersections_over_frames = list(trackId_intersections.values())
# use this in case you want them sorted, uncomment sorted_dict
#intersections_over_frames = list(sorted_dict.values())

average_number_of_frames = np.mean(intersections_over_frames)
std_deviation = np.std(intersections_over_frames)

was_seen = {}
frame_index_again = 0

for image_frame in images_metadata:
    for bb in image_frame.BoundingBoxes.BoundingBoxList:
        entity_intersect = bb.XLeftTop < entity.Rectangle.Right and entity.Rectangle.X < bb.XLeftTop + bb.Width and bb.YLeftTop < entity.Rectangle.Bottom and entity.Rectangle.Y < bb.YLeftTop + bb.Height
        event = create_event_data(bb, False, entity)

        if entity_intersect and not average_number_of_frames - std_deviation < trackId_intersections[
            bb.ClassName.TrackId] < average_number_of_frames + std_deviation:
            if bb.ClassName.TrackId not in was_seen:
                event.IsFirstEventOfObject = True
                was_seen[bb.ClassName.TrackId] = True

            if frame_index_again in events_by_frame_index:
                events_by_frame_index[frame_index_again].Add(event)
                if events_by_frame_index[frame_index_again].Count > max_frequency:
                    max_frequency = events_by_frame_index[frame_index_again].Count

            else:
                lst = List[EventData]()
                lst.Add(event)
                events_by_frame_index[frame_index_again] = lst
                if max_frequency == 0:
                    max_frequency = 1

    frame_index_again += 1

plt.bar(trackIds, intersections_over_frames)
plt.xticks(trackIds)

plt.axhline(y=average_number_of_frames, color='green', linestyle='--', label=f'Average: {average_number_of_frames:.2f}')
plt.axhline(y=average_number_of_frames + std_deviation, color='red', linestyle='dotted', label=f'Avg + Std Dev')
plt.axhline(y=average_number_of_frames - std_deviation, color='red', linestyle='dotted', label=f'Avg - Std Dev')

plt.title("Tracks intersecting an entity over multiple frames")
plt.xlabel("Track IDs")
plt.ylabel("Number of frames")
plt.legend()
plt.show(block=False)

combined_string += f"\nno.trckids:{len(trackIds)}, no.ionf:{len(intersections_over_frames)}"
string_output = combined_string