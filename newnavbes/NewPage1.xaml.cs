

namespace newnavbes;

public partial class NewPage1 : ContentPage
{
    private const string YandexApiKey = "9f4d093c-51a6-4adf-aa7b-1a48afc68077";
    private bool isSelectingStartPoint = false; // ���� ��� ������ ����� �����������
    private bool isSelectingEndPoint = false;   // ���� ��� ������ ����� ����������
    private double[] selectedStartCoords;
    private double[] selectedEndCoords;
    public RouteDrawable RouteDrawable { get; set; }
    public NewPage1()
	{
		InitializeComponent();
        LoadMap();
        RouteDrawable = new RouteDrawable();
        RouteCanvas.Drawable = RouteDrawable;
    }

    private void LoadMap()
    {
        string mapHtml = $@"
        <!DOCTYPE html>
        <html>
        <head>
            <meta charset='utf-8' />
            <meta name='viewport' content='width=device-width, initial-scale=1' />
            <title>Yandex Map</title>
            <script src='https://api-maps.yandex.ru/2.1/?lang=ru_RU&apikey={YandexApiKey}'></script>
            <style>
                body {{ margin: 0; padding: 0; }}
                #map {{ width: 100vw; height: 100vh; }}
            </style>
        </head>
        <body>
            <div id='map'></div>
            <script>
                ymaps.ready(init);
                var map, currentLocationMarker, startPointMarker, endPointMarker, userLocationCoords;
                var building, floors = {{}};

                function init() {{
                    map = new ymaps.Map('map', {{
                        center: [56.32654825435959, 44.010495293763064],
                        zoom: 17
                    }});

                    // ����������� �������������� ������������
                    if (navigator.geolocation) {{
                        navigator.geolocation.watchPosition(function (position) {{
                            var coords = [position.coords.latitude, position.coords.longitude];
                            userLocationCoords = coords;

                            if (currentLocationMarker) {{
                                currentLocationMarker.geometry.setCoordinates(coords);
                            }} else {{
                                currentLocationMarker = new ymaps.Placemark(coords, {{
                                    iconContent: '�� �����',
                                    preset: 'islands#redDotIcon'
                                }});
                                map.geoObjects.add(currentLocationMarker);
                            }}
                        }}, function (error) {{
                            console.log('������ ��� ����������� ��������������:', error);
                        }}, {{
                            enableHighAccuracy: true,
                            timeout: 5000,
                            maximumAge: 0
                        }});
                    }}

                    building = new ymaps.GeoObject({{
                        geometry: {{
                            type: 'Polygon',
                            coordinates: [
                                [[56.32654825435959, 44.010495293763064],
                                 [56.326548, 44.011495],
                                 [56.325548, 44.011495],
                                 [56.325548, 44.010495]]
                            ]
                        }},
                        properties: {{
                            hintContent: '������ � �������'
                        }},
                        options: {{
                            fillColor: '#00FF00',
                            opacity: 0.5
                        }}
                    }});

                    map.geoObjects.add(building);

                    createFloors();

                    map.events.add('click', function (e) {{
                        var coords = e.get('coords');
                        if (isSelectingStartPoint) {{
                            if (startPointMarker) {{
                                map.geoObjects.remove(startPointMarker);
                            }}
                            startPointMarker = new ymaps.Placemark(coords, {{
                                iconContent: '������',
                                preset: 'islands#blueDotIcon'
                            }});
                            map.geoObjects.add(startPointMarker);
                            isSelectingStartPoint = false;
                            selectedStartCoords = coords;

                            if (selectedEndCoords) {{
                                buildRoute(selectedStartCoords, selectedEndCoords);
                            }}
                        }} else if (isSelectingEndPoint) {{
                            if (endPointMarker) {{
                                map.geoObjects.remove(endPointMarker);
                            }}
                            endPointMarker = new ymaps.Placemark(coords, {{
                                iconContent: '����',
                                preset: 'islands#greenDotIcon'
                            }});
                            map.geoObjects.add(endPointMarker);
                            isSelectingEndPoint = false;
                            selectedEndCoords = coords;

                            if (selectedStartCoords) {{
                                buildRoute(selectedStartCoords, selectedEndCoords);
                            }} else if (userLocationCoords) {{
                                buildRoute(userLocationCoords, selectedEndCoords);
                            }}
                        }}
                    }});
                }}

                function createFloors() {{
                    for (var i = 1; i <= 5; i++) {{
                        floors[i] = new ymaps.GeoObject({{
                            geometry: {{
                                type: 'Polygon',
                                coordinates: [
                                    [[56.32654825435959, 44.010495293763064],
                                     [56.326548, 44.011495],
                                     [56.325548, 44.011495],
                                     [56.325548, 44.010495]]
                                ]
                            }},
                            properties: {{
                                hintContent: '���� ' + i
                            }},
                            options: {{
                                fillColor: '#' + (i * 2).toString(16).padStart(2, '0') + 'FF00',
                                opacity: 0.5,
                                visible: false
                            }}
                        }});

                        map.geoObjects.add(floors[i]);
                    }}
                }}

                function showFloor(floorNumber) {{
                    for (var i = 1; i <= 5; i++) {{
                        floors[i].options.set('visible', i === floorNumber);
                    }}
                    selectedEndCoords = floors[floorNumber].geometry.getCoordinates()[0];
                    if (selectedStartCoords) {{
                        buildRoute(selectedStartCoords, selectedEndCoords);
                    }} else if (userLocationCoords) {{
                        buildRoute(userLocationCoords, selectedEndCoords);
                    }}
                }}

                function buildRoute(startCoords, endCoords) {{
                    ymaps.route([startCoords, endCoords]).then(function (route) {{
                        map.geoObjects.add(route);
                    }}, function (error) {{
                        alert('�� ������� ��������� �������: ' + error.message);
                    }});
                }}

                window.external = {{
                    locateUser: function() {{
                        // ���� ��� ���������� ��� ����������� �������������� ������������
                    }},
                    selectStartPoint: function() {{
                        isSelectingStartPoint = true;
                        alert('�������� �� �����, ����� ������� ����� �����������');
                    }},
                    selectEndPoint: function() {{
                        isSelectingEndPoint = true;
                        alert('�������� �� �����, ����� ������� ����� ����������');
                    }},
                    buildRouteFromCurrentLocation: function() {{
                        if (userLocationCoords && endPointMarker) {{
                            var destinationCoords = endPointMarker.geometry.getCoordinates();
                            buildRoute(userLocationCoords, destinationCoords);
                        }} else {{
                            alert('���������� ������� ����� ���������� �/��� ���������� ������� ��������������');
                        }}
                    }},
                    selectFloor: function(floorNumber) {{
                        showFloor(floorNumber);
                    }}
                }};
            </script>
        </body>
        </html>
        ";

        mapView.Source = new HtmlWebViewSource
        {
            Html = mapHtml
        };
    }

    // ����������� ��� ������
    private void OnLocateUserPosition(object sender, EventArgs e)
    {
        mapView.Eval("window.external.locateUser();");
    }

    private void OnSelectStartPoint(object sender, EventArgs e)
    {
        mapView.Eval("window.external.selectStartPoint();");
    }

    private void OnSelectEndPoint(object sender, EventArgs e)
    {
        mapView.Eval("window.external.selectEndPoint();");
    }

    private void OnBuildRouteFromCurrentLocation(object sender, EventArgs e)
    {
        mapView.Eval("window.external.buildRouteFromCurrentLocation();");
    }


    private void OnFloorButtonClicked(object sender, EventArgs e)
    {
        Button button = (Button)sender;
        int floorNumber = int.Parse(button.CommandParameter.ToString());

        // ��������� ���� ��� ��������
        RouteDrawable.TargetFloor = floorNumber;
        RouteCanvas.Invalidate(); // ��������� Canvas ��� ����������
    }

}

public class RouteDrawable : IDrawable
{
    public int TargetFloor { get; set; }

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
        // ������� ���������� ��������
        canvas.FillColor = Colors.White;
        canvas.FillRectangle(dirtyRect);

        // ���������� �����
        PointF entrance = new PointF(50, 350);

        // ���������� ������ (������)
        PointF[] floors = new PointF[]
        {
            new PointF(50, 350), // ���� 1 (����)
            new PointF(100, 250), // ���� 2 (����� ������)
            new PointF(50, 150), // ���� 3 (����� �����)
            new PointF(100, 100), // ���� 4 (����� ������)
            new PointF(50, 50)   // ���� 5 (����� �����)
        };

        // �������� ��������� TargetFloor
        if (TargetFloor < 1 || TargetFloor > floors.Length)
        {
            // �������, ���� ���� �� ����������
            return;
        }

        // ������ ����
        canvas.FillColor = Colors.Green;
        canvas.FillCircle(entrance, 10);

        // ������ ���� �� ��������� �����
        canvas.FillColor = Colors.Red;
        canvas.FillCircle(floors[TargetFloor - 1], 10);

        // ������ �������
        canvas.StrokeColor = Colors.Blue;
        canvas.StrokeSize = 2;

        // ��������� ����� ��������
        PointF currentPoint = entrance;

        // �������� �� ������ �� ����������
        for (int i = 1; i <= TargetFloor - 1; i++)
        {
            PointF nextPoint = floors[i];

            // ��������� ����������� �� ������ �����, ����� ������� ��� �����
            canvas.DrawLine(currentPoint, new PointF(currentPoint.X, nextPoint.Y)); // ������������ ����� �����
            canvas.DrawLine(new PointF(currentPoint.X, nextPoint.Y), nextPoint); // �������������� ����� � �������

            // ��������� ������� �����
            currentPoint = nextPoint;
        }
    }
}