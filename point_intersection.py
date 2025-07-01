from Snowman.Data import EventData
from System.Collections.Generic import List

intersections = 0
intersected_track_ids = {}
frame_index = 0

for image_frame in images_metadata:
    for bounding_box in image_frame.BoundingBoxes.BoundingBoxList:
        entity_intersect = entity.Position.X >= bounding_box.XLeftTop and entity.Position.X <= bounding_box.XLeftTop + bounding_box.Width and entity.Position.Y >= bounding_box.YLeftTop and entity.Position.Y <= bounding_box.YLeftTop + bounding_box.Height
        event = create_event_data(bounding_box, False, entity)
 
        if entity_intersect:
            if bounding_box.ClassName.TrackId not in intersected_track_ids:
                event.IsFirstEventOfObject = True
                intersected_track_ids[bounding_box.ClassName.TrackId] = True
                intersections += 1
            
            if frame_index in events_by_frame_index:
                events_by_frame_index[frame_index].Add(event)
                if events_by_frame_index[frame_index].Count > max_frequency:
                    max_frequency = events_by_frame_index[frame_index].Count
            else:
                lst = List[EventData]()
                lst.Add(event)
                events_by_frame_index[frame_index] = lst
                if max_frequency == 0:
                    max_frequency = 1
                
    frame_index += 1

string_output = "Number of times this point has intersected with a bounding box: {}".format(intersections)