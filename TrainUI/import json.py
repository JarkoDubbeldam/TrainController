import json
from pydantic import BaseModel

class Point(BaseModel):
    x: int
    y: int

class Segment(BaseModel):
    point_a: Point
    control_point_a: Point
    point_b: Point
    control_point_b: Point
    segment_id: int
    section_id: int

with open("appstate.json","r") as f:
    data=json.load(f)

track = data["Track"]
layout = json.loads(track["LayoutJson"])

temp = [
    (b["Id"], c["ToBoundaryId"], c["ViaSection"]["Id"], c["ViaSection"]["SectionId"]) 
    for b in layout["Boundaries"] 
    for c in b["Connections"]
]

def build_point(data):
    try:
        return Point(x=data["X"], y=data["Y"])
    except KeyError:
        print(data)
        raise

def build_segment(from_id, to_id, segment_id, section_id):
    from_point = build_point(track["BoundaryLocations"][str(from_id)]["Location"])
    to_point = build_point(track["BoundaryLocations"][str(to_id)]["Location"])
    control_points = track["SectionLocations"][str(segment_id)]
    from_control_point = build_point(control_points["ControlPoint1"])
    to_control_point = build_point(control_points["ControlPoint2"])
    return Segment(point_a=from_point, control_point_a=from_control_point, point_b=to_point, control_point_b=to_control_point, section_id=section_id, segment_id=segment_id)

segments = set()
results = []
for segment in temp:
    if segment[2] not in segments:
        results.append(build_segment(*segment))
        segments.add(segment[2])

class Everything(BaseModel):
    data: list[Segment]

Everything(data=results).model_dump_json()