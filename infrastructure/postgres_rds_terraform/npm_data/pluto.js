var Bytes = require('nyc-bytes');
var pg = require('pg');
var stdio = require('stdio');
var proj4 = require('proj4');
var fs = require('fs');
var os = require('os');
var path = require('path');
var copy_from = require('pg-copy-streams').from;
var CSV_SEPARATOR = '|';

var opts = stdio.getopt({
  'host': {key: 'host', args: 1, default: 'localhost', description: 'Hostname for postgres'},
  'port': {key: 'port', args: 1, default: 5432, description: 'Port for postgres'},
  'user': {key: 'user', args: 1, default: 'postgres', description: 'User for postgres'},
  'password': {key: 'password', args: 1, default: '', description: 'Password for user'},
  'database': {key: 'database', args: 1, default: 'postgres', description: 'Database name in postgres'},
  'csv_file': {key: 'csv_file', args: 1, default: null, description: 'Store dump as CSV file'},
  'boroughs': {key: 'boroughs', args: "*", default: null, description: 'Dump only these boroughs'}
});

// Use only csv or insert to database?
var only_csv = false;
if (opts.csv_file) {
  only_csv = true;
} else {
  var hrtime = process.hrtime();
  opts.csv_file = os.tmpdir() + '/dump-' + (hrtime[0] * 1000000 + hrtime[1] / 1000) + '.csv';
}

// Set defaults for boroughs
if (!opts.boroughs) {
  opts.boroughs = ['MN','BX','BK','QN','SI'];
}

var csv_stringify = function (data) {
  return data.map(function (value) {
    if (typeof(value) === 'string') {
      value = value.trim();
      if (value.indexOf(CSV_SEPARATOR) > -1) {
        return '"' + value + '"';
      }
    }
    return value;
  }).join(CSV_SEPARATOR) + "\n";
};

var csv_stream = fs.createWriteStream(path.resolve(opts.csv_file));
console.log('Using csv file for dump: ' + opts.csv_file);

csv_stream.once('open', function() {
  var dataset = Bytes.Pluto;
  console.log('init');
  dataset.init().then(() => {
    console.log('Dataset ready.');

    // Add CSV header
    csv_stream.write(csv_stringify([
      "Borough", "Block", "Lot", "CD", "CT2010", "CB2010",
      "SchoolDist", "Council", "ZipCode", "FireComp", "PolicePrct",
      "HealthArea", "SanitBoro", "SanitDistrict", "SanitSub", "Address",
      "ZoneDist1", "ZoneDist2", "ZoneDist3", "ZoneDist4", "Overlay1",
      "Overlay2", "SPDist1", "SPDist2", "SPDist3", "LtdHeight", "SplitZone",
      "BldgClass", "LandUse", "Easements", "OwnerType", "OwnerName",
      "LotArea", "BldgArea", "ComArea", "ResArea", "OfficeArea", "RetailArea",
      "GarageArea", "StrgeArea", "FactryArea", "OtherArea", "AreaSource",
      "NumBldgs", "NumFloors", "UnitsRes", "UnitsTotal", "LotFront",
      "LotDepth", "BldgFront", "BldgDepth", "Ext", "ProxCode", "IrrLotCode",
      "LotType", "BsmtCode", "AssessLand", "AssessTot", "ExemptLand",
      "ExemptTot", "YearBuilt", "YearAlter1", "YearAlter2", "HistDist",
      "Landmark", "BuiltFAR", "ResidFAR", "CommFAR", "FacilFAR",
      "BoroCode", "BBL", "CondoNo", "Tract2010", "XCoord", "YCoord",
      "ZoneMap", "ZMCode", "Sanborn", "TaxMap", "EDesigNum", "APPBBL",
      "APPDate", "PLUTOMapID", "Version", "Latitude", "Longitude"
    ]));

    var stream = dataset.stream();
    stream.on('data', (row) => {
      // Skip empty coordinates record
      if (!row || !row.XCoord || !row.YCoord || !row.SanitBoro) {
        return;
      }

      if (opts.boroughs.indexOf(row.Borough) === -1) {
        return;
      }

      var NAD83 = '+proj=lcc +lat_1=41.03333333333333 +lat_2=40.66666666666666 +lat_0=40.16666666666666 +lon_0=-74 +x_0=300000.0000000001 +y_0=0 +ellps=GRS80 +datum=NAD83 +to_meter=0.3048006096012192 +no_defs';
      var WGS84 = 'EPSG:4326';

      var x = parseInt(row.XCoord);
      var y = parseInt(row.YCoord)
      var coords = proj4(NAD83 ,WGS84, [x,y]);

      row.Longitude = coords[0];
      row.Latitude = coords[1];

      console.log(x, y, coords[0], coords[1]);
      csv_stream.write(csv_stringify([
          row.Borough, row.Block, row.Lot,
          row.CD, row.CT2010, row.CB2010,
          row.SchoolDist, row.Council, row.ZipCode,
          row.FireComp, row.PolicePrct, row.HealthArea,
          row.SanitBoro, row.SanitDistrict, row.SanitSub,
          row.Address.trim(), row.ZoneDist1, row.ZoneDist2,
          row.ZoneDist3, row.ZoneDist4, row.Overlay1,
          row.Overlay2, row.SPDist1, row.SPDist2,
          row.SPDist3, row.LtdHeight, row.SplitZone,
          row.BldgClass, row.LandUse, row.Easements,
          row.OwnerType, row.OwnerName, row.LotArea,
          row.BldgArea, row.ComArea, row.ResArea,
          row.OfficeArea, row.RetailArea, row.GarageArea,
          row.StrgeArea, row.FactryArea, row.OtherArea,
          row.AreaSource, row.NumBldgs, row.NumFloors,
          row.UnitsRes, row.UnitsTotal, row.LotFront,
          row.LotDepth, row.BldgFront, row.BldgDepth, row.Ext,
          row.ProxCode, row.IrrLotCode, row.LotType,
          row.BsmtCode, row.AssessLand, row.AssessTot,
          row.ExemptLand, row.ExemptTot, row.YearBuilt,
          row.YearAlter1, row.YearAlter2, row.HistDist,
          row.Landmark, row.BuiltFAR, row.ResidFAR,
          row.CommFAR, row.FacilFAR, row.BoroCode, row.BBL,
          row.CondoNo, row.Tract2010, row.XCoord, row.YCoord,
          row.ZoneMap, row.ZMCode, row.Sanborn, row.TaxMap,
          row.EDesigNum, row.APPBBL, row.APPDate,
          row.PLUTOMapID, row.Version, row.Latitude,
          row.Longitude
        ]));
    });
    stream.on('end', () => {
      csv_stream.end(function () {
        if (only_csv) {
          console.log('Data saved to CSV file.');
          return;
        }

        var pool = new pg.Pool({
          user: opts.user,
          password: opts.password,
          host: opts.host,
          port: opts.port,
          database: opts.database
        });
        pool.connect(function(err, client, done) {
          if (err) {
            console.log(err);
            return;
          }

          client.query('CREATE TABLE IF NOT EXISTS public.dbversion ("Id" int4 NULL, "Database" varchar NULL, "Version" varchar NULL, "date" date NULL, buildstart date NULL, buildend date NULL)', function (err) {
            if (err) {
              console.log(err);
              return;
            }
          });

          client.query('TRUNCATE public."PLUTO"; ALTER SEQUENCE public."PLUTO_id_seq" START WITH 1', function (err) {
            if (err) {
              console.log(err);
              return;
            }

            var fstream = fs.createReadStream(opts.csv_file);
            var dbstream = client.query(copy_from('COPY public."PLUTO"("Borough", "Block", "Lot", "CD", "CT2010", "CB2010", "SchoolDist", "Council", "ZipCode", "FireComp", "PolicePrct", "HealthArea", "SanitBoro", "SanitDistrict", "SanitSub", "Address", "ZoneDist1", "ZoneDist2", "ZoneDist3", "ZoneDist4", "Overlay1", "Overlay2", "SPDist1", "SPDist2", "SPDist3", "LtdHeight", "SplitZone", "BldgClass", "LandUse", "Easements", "OwnerType", "OwnerName", "LotArea", "BldgArea", "ComArea", "ResArea", "OfficeArea", "RetailArea", "GarageArea", "StrgeArea", "FactryArea", "OtherArea", "AreaSource", "NumBldgs", "NumFloors", "UnitsRes", "UnitsTotal", "LotFront", "LotDepth", "BldgFront", "BldgDepth", "Ext", "ProxCode", "IrrLotCode", "LotType", "BsmtCode", "AssessLand", "AssessTot", "ExemptLand", "ExemptTot", "YearBuilt", "YearAlter1", "YearAlter2", "HistDist", "Landmark", "BuiltFAR", "ResidFAR", "CommFAR", "FacilFAR", "BoroCode", "BBL", "CondoNo", "Tract2010", "XCoord", "YCoord", "ZoneMap", "ZMCode", "Sanborn", "TaxMap", "EDesigNum", "APPBBL", "APPDate", "PLUTOMapID", "Version", "Latitude", "Longitude") FROM STDIN DELIMITER \'' + CSV_SEPARATOR + '\' CSV HEADER'));

            var on_error = function (err) {
              console.log(err);
              done();
            };

            var on_success = function () {
              console.log("Dataset exported to database.");
              done();
            };
            fstream.on('error', on_error);
            fstream.pipe(dbstream).on('finish', on_success).on('error', on_error);
          });
        });
      });


    });
  }).catch((err) => {
    console.error(err);
  });
});
