import bpy
import json
import argparse
import sys
import os
from pathlib import Path  

ROOT_FOLDER = os.path.dirname(os.path.realpath(__file__))


def preprocess_fbx(in_file_path, out_folder_path, out_name):

    # Clear file
    bpy.ops.object.select_all(action = 'SELECT')
    bpy.ops.object.delete()

    # Open fbx into blender
    bpy.ops.import_scene.fbx(filepath=in_file_path)

    # For each blender object in BakedMesh > Root
    quill_root = bpy.data.objects["Root"]

    layer_hierarchy = {}
    for child in quill_root.children:
        bpy.ops.object.select_all(action='DESELECT')
        child.select_set(True)
        bpy.ops.mesh.separate(type='LOOSE')
        child_parts = bpy.context.view_layer.objects.selected
        # ignore empty layers
        # Note: here would be a good place to filter out unwanted layers, eg reference image layers
        if len(child_parts) > 0:
            print(f"Layer = {child.name}, # strokes = {len(child_parts)}")
            layer_hierarchy[child.name] = []
            for part in child_parts:
                # print(part.name)
                layer_hierarchy[child.name].append(part.name)
        child.select_set(False)

    
    with open(os.path.join(out_folder_path, f"{out_name}.json"), 'w') as f:
        f.write(json.dumps(layer_hierarchy, indent=4))

    bpy.ops.export_scene.fbx(filepath=os.path.join(out_folder_path, f"{out_name}.fbx"))


if __name__ == "__main__":

    parser = argparse.ArgumentParser()

    parser.add_argument('-i', '--input', type=str, default=None, required=True, help="Input FBX file path (relative to this file)")
    parser.add_argument('-o', '--out', type=str, default=None, required=True, help="Output folder path")

    # get the args passed to blender after "--", all of which are ignored by
    # blender so scripts may receive their own arguments
    argv = sys.argv
    if "--" not in argv:
        argv = []  # as if no args are passed
    else:
        argv = argv[argv.index("--") + 1:]  # get all args after "--"

    args = parser.parse_args(argv)

    preprocess_fbx(
        args.input,
        args.out,
        out_name=Path(args.input).stem
    )
