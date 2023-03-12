using GMap.NET;
using GMap.NET.MapProviders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GMap.Net_Wpf_Sample.CustomMapProviders {
    internal class AMapProvider : GMapProvider {


        private string urlFormat = "https://wprd04.is.autonavi.com/appmaptile?x={0}&y={1}&z={2}&lang=zh_cn&size=1&scale=1&style=7";
        public static AMapProvider Instance => new AMapProvider();


        public AMapProvider() {
            Copyright = "高德地图";
            RefererUrl = "http://www.amap.com";
        }

        private GMapProvider[] _overlays;

        private readonly Guid id = Guid.Parse("98f57982-65cc-442c-a820-d8092673d98a");

        public override Guid Id => id;

        public override string Name => "高德地图";

        public override PureProjection Projection => GMap.NET.Projections.MercatorProjection.Instance;

        public override GMapProvider[] Overlays {
            get {
                if (_overlays == null) {
                    _overlays = new GMapProvider[] { this };
                }

                return _overlays;
            }
        }


        public override PureImage GetTileImage(GPoint pos, int zoom) {
            var url = MakeTileImageUrl(pos, zoom);
            return GetTileImageUsingHttp(url);
        }

        private string MakeTileImageUrl(GPoint pos, int zoom) {
            string url = string.Format(urlFormat, pos.X, pos.Y, zoom);

            return url;
        }


    }
}
