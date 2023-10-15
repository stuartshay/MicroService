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

    if layer.isValid():
        # Extract unique colors from the GeoJSON data
        color_to_type = {feature['properties']['markerColor']: feature['properties']['type'] for feature in geojson_data['features']}

        # Create a list to store category-symbol pairs
        categories = []
        for color, type_name in color_to_type.items():
            # Define the SVG path for the marker based on the color
            svg_marker_path = os.path.join(project_directory, 'markers', f"{color}.svg")
            svg_symbol = QgsSvgMarkerSymbolLayer(svg_marker_path)
            svg_symbol.setSize(10)  # Adjust the size as needed

            # Create a new symbol with the SVG symbol layer
            symbol = QgsMarkerSymbol.createSimple({})
            symbol.changeSymbolLayer(0, svg_symbol)

            # Define a category for this color
            category = QgsRendererCategory(color, symbol, type_name)
            categories.append(category)

        # Create a categorized renderer and assign it to the layer
        renderer = QgsCategorizedSymbolRenderer('markerColor', categories)
        layer.setRenderer(renderer)

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
