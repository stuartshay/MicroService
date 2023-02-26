﻿using MicroService.Service.Helpers;
using MicroService.Service.Models.Base;
using MicroService.Service.Models.Enum;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using System;
using System.Collections.Generic;

namespace MicroService.Service.Services
{
    public delegate IShapefileDataReaderService ShapefileDataReaderResolver(string key);

    public abstract class AbstractShapeService<T> where T : class
    {
        public IShapefileDataReaderService ShapeFileDataReader { get; internal set; }

        public ShapefileHeader GetShapeProperties()
        {
            ShapefileHeader shpHeader = ShapeFileDataReader.ShapeHeader;
            return shpHeader;
        }

        public DbaseFileHeader GetShapeDatabaseProperties()
        {
            DbaseFileHeader header = ShapeFileDataReader.DbaseHeader;
            return header;
        }

        public abstract T GetFeatureLookup(double x, double y);

        public abstract IEnumerable<T> GetFeatureLookup(List<KeyValuePair<string, object>> attributes);

        public string GetFeatureName(string propertyName)
        {
            T shapeClass = Activator.CreateInstance<T>();
            var featureName = ReflectionExtensions.GetAttributeFromProperty<FeatureNameAttribute>(shapeClass, propertyName);

            return featureName?.AttributeName;
        }

        public ShapeBase GetShapeBaseFeatureLookup(double x, double y)
        {
            var shape = new ShapeBase();
            return shape;
        }

        /// <summary>
        /// Validate/Map Shape Feature Properties
        /// </summary>
        /// <param name="attributes"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, object>> ValidateFeatureKey(List<KeyValuePair<string, object>> attributes)
        {
            for (int i = 0; i < attributes.Count; i++)
            {
                var key = attributes[i].Key;
                var featureName = GetFeatureName(key);
                attributes[i] = new KeyValuePair<string, object>(featureName, attributes[i].Value);
            }

            return attributes;
        }

        public IReadOnlyCollection<Feature> GetFeatures() =>
                ShapeFileDataReader.GetFeatures();
    }
}
