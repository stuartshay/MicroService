# app.py - this should run automativly  when the docker is Built and Run
import os
print ('[info] - running script in gdal container \n')
print (r"[info] - /from_shapefile/",os.listdir(r'/scripts/from_shapefile'))

# find the *.shp file
for f in os.listdir(r'/scripts/from_shapefile') :
    if f.split('.')[-1] == 'shp':
        print ('[info] - found shpfile:',f)
        shp_file =  f
        # the name of the json file is the same name as the shp file
        json_file = f.split('.')[0] + '.json' 
        cmd = f'ogr2ogr -f GeoJSON /scripts/to_GeoJSON/{json_file} /scripts/from_shapefile/{shp_file}'
        print ('[info] - runing: ', cmd)
        os.system(cmd)
                
print (r"[info] - /to_GeoJSON/",os.listdir(r'/scripts/to_GeoJSON'))
