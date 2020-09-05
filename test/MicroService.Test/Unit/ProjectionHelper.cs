using System;
using System.Collections.Generic;
using System.Text;

namespace MicroService.Test.Unit
{
    public static class ProjectionHelper
    {
        //private readonly static CoordinateTransformFactory ctFactory = new CoordinateTransformFactory();
        //private readonly static CoordinateReferenceSystemFactory csFactory = new CoordinateReferenceSystemFactory();
        //private static readonly CoordinateReferenceSystem srcCRS;
        //private static readonly CoordinateReferenceSystem tgtCRS;

        //static ProjectionHelper()
        //{
        //    srcCRS = CreateCS("ESRI:102718");  //http://www.spatialreference.org/ref/esri/102718/
        //    tgtCRS = CreateCS("EPSG:4326");    // WGS 84
        //}

        //public static ProjCoordinate TransformProjection(ProjCoordinate projCoordinate)
        //{
        //    ProjCoordinate projCoordinateTarget = new ProjCoordinate();

        //    ICoordinateTransform trans = ctFactory.CreateTransform(srcCRS, tgtCRS);
        //    trans.Transform(projCoordinate, projCoordinateTarget);

        //    return projCoordinateTarget;

        //}

        //private static CoordinateReferenceSystem CreateCS(String csSpec)
        //{
        //    CoordinateReferenceSystem cs = null;
        //    // test if name is a PROJ4 spec
        //    if (csSpec.IndexOf("+") >= 0)
        //    {
        //        cs = csFactory.CreateFromParameters("Anon", csSpec);
        //    }
        //    else
        //    {
        //        cs = csFactory.CreateFromName(csSpec);
        //    }
        //    return cs;
        //}

    }
}
