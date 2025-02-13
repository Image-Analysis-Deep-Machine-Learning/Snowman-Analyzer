intersections = 0
intersected_track_ids = {}


for image_frame in images_metadata:
    for bounding_box in image_frame.BoundingBoxes.BoundingBoxList:
        entity_intersect = entity.Position.X >= bounding_box.XLeftTop and entity.Position.X <= bounding_box.XLeftTop + bounding_box.Width and entity.Position.Y >= bounding_box.YLeftTop and entity.Position.Y <= bounding_box.YLeftTop + bounding_box.Height

        if entity_intersect:
            if bounding_box.ClassName.TrackId in intersected_track_ids:
                continue
            else:
                intersected_track_ids[bounding_box.ClassName.TrackId] = True
                intersections += 1


string_output = "Number of times this point has intersected with a bounding box: {}".format(intersections)