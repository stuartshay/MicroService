import requests
import os
import json
from qgis.core import (
    QgsVectorLayer,
    QgsProject,
    QgsCoordinateReferenceSystem,
    QgsPalLayerSettings,
    QgsVectorLayerSimpleLabeling
)
from PyQt5.QtGui import QFont, QColor

def apply_labels(layer, attribute_name):
    """Apply labels to a given layer based on an attribute."""
    labeling = QgsPalLayerSettings()
    labeling.enabled = True  # Turn on labeling
    labeling.fieldName = attribute_name
    text_format = labeling.format()
    text_format.setFont(QFont("Arial", 10))
    text_format.setColor(QColor("black"))
    labeling.setFormat(text_format)
    labeling.placement = QgsPalLayerSettings.OverPoint
    layer.setLabeling(QgsVectorLayerSimpleLabeling(labeling))
    layer.setLabelsEnabled(True)


# Get the directory where the QGIS project file is saved
project_directory = os.path.dirname(QgsProject.instance().fileName())

# Base URL
base_url = 'https://navigator-maps-api-w6zlqlyoma-uk.a.run.app/api/Maps/6510befbf43b01079b01ea65'

# Query parameters
params = {
    'PhotoSize': 'thumbnail',
    'PhotoDisplay': 'all',
    'MapFormat': 'geojson'
}

# Headers
headers = {
    'accept': '*/*'
}

# Make the GET request
response = requests.get(base_url, headers=headers, params=params)

# Print the status code and a snippet of the response to the console
print(f"Response Status Code: {response.status_code}")
print(f"Response Text (First 500 characters): {response.text[:500]}")

# Check if the response is successful
if response.status_code == 200:
    geojson_data = response.json()

    # Convert the GeoJSON data to a string and create a QGIS vector layer
    geojson_string = json.dumps(geojson_data)
    uri = f"GeoJSON:{geojson_string}"
    layer = QgsVectorLayer(uri, "My GeoJSON Layer", "ogr")

    # Specify the CRS if it's known (e.g., "EPSG:4326" for WGS 84)
    layer.setCrs(QgsCoordinateReferenceSystem("EPSG:4326"))

    # Check if the layer is valid
    if layer.isValid():

        layer.setName("Landmarks")
        svg_path = os.path.join(project_directory, 'markers', 'blue_marker.svg')
        print(svg_path)
        symbol = QgsSvgMarkerSymbolLayer(svg_path)
        symbol.setSize(10)

        # Get the renderer and the existing symbol
        renderer = layer.renderer()
        existing_symbol = renderer.symbol()

        # Replace the default symbol layer with the new SVG symbol
        existing_symbol.changeSymbolLayer(0, symbol)

        # Apply labels using the helper class
        apply_labels(layer, "Name")

        # Define the HTML content for the map tip
        html_content = """
        <b>[%"Name"%]</b><br>
        [%"Description"%]
        """

        # Set the map tip for the layer
        layer.setMapTipTemplate(html_content)

        # Refresh the map and add the layer to the QGIS project
        layer.triggerRepaint()
        QgsProject.instance().addMapLayer(layer)

    else:
        print("Failed to load layer")
        print(layer.error().message())
else:
    print("Error:", response.status_code, response.text)

