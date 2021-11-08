# app.py - this should run automativly in when the docker is built and run

import subprocess
import sys
import os

# check if we have args in the python call eg : app.py path_to_shp_folder path_to_json_folder
if len(sys.argv)==2 : 
    # no paths found so we are going to set the default path of the docker \tmp
    pass
    
else:
    path_to_shp_folder = r"\tmp\Shape"
    path_to_json_folder = r"\tmp\GeoJSON"
        

# find the *.shp file
for f in os.listdir(path_to_shp_folder) :
    if f.split('.')[-1] = 'shp'
    path_to_shp_file = path_to_shp_folder + "//" + f
    path_to_json_file = path_to_json_folder + "//" + f[0] + '.json' 
    cmd = 'ogr2ogr -f GeoJSON"
    subprocess.run([cmd, path_to_json_file, path_to_shp_file], capture_output=True)
