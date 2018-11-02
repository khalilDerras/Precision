using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TopoSurf.DataModels;
using TopoSurf.Pages;
using TopoSurf.ViewModel.Base;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using static System.IO.File;
using static System.IO.Stream;
using static System.IO.FileMode;
using static System.IO.StreamReader;
using static System.IO.StreamWriter;
using static System.IO.TextReader;
using static System.IO.TextWriter;
using TopoSurf.MessageBoxStyle;

namespace TopoSurf
{
    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : Page
    {
        public static double leftMargin=22,rightMargin=125;
        List<Tuple<List<Point>, PathGeometry, List<Point>, float>> allCurvesData = new List<Tuple<List<Point>, PathGeometry, List<Point>, float>>();
        Stack<Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>> actions = new Stack<Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>>();
        Stack<Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>> actionsRedo = new Stack<Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>>();
        float altitudee;

        /* here the data of each curve will be held in a list , that will be paired (in a tuple ) with 
         * the corresponding path figure , the third item being the tangentList then the altitude
         * then all the data will be put in a global list*/

        List<Point> currentCurveCtrlPts = new List<Point>();
        Path path = new Path();                      // a path that holds the geometry off all curves
        const double curveComputingStep = 0.0005;
        PathGeometry geoPath = new PathGeometry();
        PathFigure temporaryFigure;
        Point mousePos = new Point();
        List<Ellipse> shownPts = new List<Ellipse>();
        int currentChildren = 0;
        int UndoCtrlPt;
        List<Triangle> triangles3D = new List<Triangle>();
        private Slider _Slider;

        public MainPage()
        {
            MainWindow.page = this;
            System.Threading.Thread.Sleep(1000); //splash screen 
            InitializeComponent();
            this.DataContext = new MainPageModel(this);
            path.Data = geoPath;
            canvas.Children.Add(path);
            path.Fill = null;
            path.Stroke = Brushes.Black;
            path.StrokeThickness = 3;

            //Setup a transform group that we'll use to manage panning of the image area
            TransformGroup group = new TransformGroup();
            ScaleTransform st = new ScaleTransform();
            group.Children.Add(st);
            TranslateTransform tt = new TranslateTransform();
            group.Children.Add(tt);
            //Wire up the slider to the image for zooming
            _Slider = ZoomSlider;
            _Slider.ValueChanged += ZoomSlider_ValueChanged;
            st.ScaleX = _Slider.Value;
            st.ScaleY = _Slider.Value;
            canvas.RenderTransformOrigin = new Point(0, 0);
            canvas.RenderTransform = group;
        }
           
        public static double Distance(Point p1, Point p2)
        {//distance between two points on the plan
            return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) + Math.Pow((p1.Y - p2.Y), 2));
        }
        void DrawBezierCurve(bool defenitive, bool replace = true,float altitude=0.0f)
        {//new curve
                List<Point> tangentPts = new List<Point>();
                double distance1, distance2; //distance 1 is from previous to current, 2 from current to next
                Point previousPoint, nextPoint, currentPoint, middlePoint1, middlePoint2, InterpolationPoint;
                double midPointsDistance, interpolatedX, interpolatedY, k;
                int previousIndex, nextIndex;

                for (int i = 0; i < currentCurveCtrlPts.Count; i++)
                {
                    currentPoint = currentCurveCtrlPts[i];
                    previousIndex = (i == 0) ? i : i - 1;
                    nextIndex = (i == currentCurveCtrlPts.Count - 1) ? i : (i + 1);
                    previousPoint = currentCurveCtrlPts[previousIndex];
                    nextPoint = currentCurveCtrlPts[nextIndex];
                    middlePoint1 = new Point((previousPoint.X + currentPoint.X) / 2, (previousPoint.Y + currentPoint.Y) / 2);
                    middlePoint2 = new Point((nextPoint.X + currentPoint.X) / 2, (nextPoint.Y + currentPoint.Y) / 2);
                    distance1 = Distance(previousPoint, currentPoint);
                    distance2 = Distance(currentPoint, nextPoint);
                    midPointsDistance = Distance(middlePoint1, middlePoint2);
                    if (distance1 + distance2 == 0)
                    {
                        k = 0;
                    }
                    else
                    {
                        k = (distance1 / (distance1 + distance2));
                    }
                    interpolatedX = middlePoint1.X + (middlePoint2.X - middlePoint1.X) * k;
                    interpolatedY = middlePoint1.Y + (middlePoint2.Y - middlePoint1.Y) * k;
                    InterpolationPoint = new Point(interpolatedX, interpolatedY);
                    Vector v = new Vector(currentPoint.X - interpolatedX, currentPoint.Y - interpolatedY);
                    tangentPts.Add(Point.Add(middlePoint1, v));
                    tangentPts.Add(Point.Add(middlePoint2, v));
                }
                //two non-relevant tangents have been created , we shall ommit them
                tangentPts.RemoveAt(0);
                tangentPts.RemoveAt(tangentPts.Count - 1);
                DrawBezierCurve(currentCurveCtrlPts, tangentPts, defenitive, replace, altitude);
        }

        Path DrawBezierCurve(List<Point> ctrlPts, List<Point> tangentPts, bool definitive, bool replace = true, float altitude = 0.0f)
        {//draw a curve from all the data
                PathFigure currentFigure = new PathFigure();
                int i2 = 0;          // will be used as a tangent index
                currentFigure.StartPoint = new Point(ctrlPts[0].X, ctrlPts[0].Y);
                for (int i = 0; i < ctrlPts.Count - 1; i++)
                {
                    currentFigure.Segments.Add(new BezierSegment(tangentPts[i2], tangentPts[i2 + 1], ctrlPts[i + 1], true));
                    i2 += 2;
                }
                geoPath.Figures.Add(currentFigure);
                if (definitive)  // we shall keep this curve there
                {
                    if (replace) 
                    {
                        allCurvesData.Add(new Tuple<List<Point>, PathGeometry, List<Point>, float>(ctrlPts, geoPath, tangentPts, altitude));
                    //push the action to the undo stack "ctrl+z"
                    actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("draw", allCurvesData.Count - 1, 0, 0, new Point(0, 0), new Point(0, 0), path, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                    }
                    else// if we're modifying an exisiting curve
                    {
                        allCurvesData[IndPath] = new Tuple<List<Point>, PathGeometry, List<Point>, float>(ctrlPts, geoPath, tangentPts, altitude);
                        ((Path)selectedPath).Data = geoPath;
                        ((Path)selectedPath).Margin = new Thickness(0, 0, 0, 0);
                        margin = new Thickness(0, 0, 0, 0);
                        canvas.Children.Remove(path);
                    }

                }
                else
                {
                    temporaryFigure = currentFigure;
                    return path;
                }
            //prepare any next change
            currentCurveCtrlPts = new List<Point>();     // for the next task
            Path copyForReturn = path;
            geoPath = new PathGeometry();
            path = new Path();
            canvas.Children.Add(path);
            path.Stroke = Brushes.Black;
            path.Fill = null;
            path.StrokeThickness = 3;
            path.Data = geoPath;
            return copyForReturn;
        }

        private void MouseDownDraw(MouseButtonEventArgs e)
        {
            
            Point p = new Point(e.GetPosition(this.canvas).X, e.GetPosition(this.canvas).Y);
            double ellipseSize = 10;
            currentCurveCtrlPts.Add(p);
            UndoCtrlPt = currentChildren;
            Ellipse ell = new Ellipse();
            ell.Width = ellipseSize;
            ell.Height = ellipseSize;
            ell.Fill = Brushes.Red;
            ell.Stroke = Brushes.Transparent;
            Canvas.SetLeft(ell, p.X - ellipseSize / 2);
            Canvas.SetTop(ell, p.Y - ellipseSize / 2);
            shownPts.Add(ell);
            canvas.Children.Add(ell);
        }
        private void MouseDownModify(MouseButtonEventArgs e)
        {
            object TestPanelOrUI = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;//test the element we clicked on
            if (TestPanelOrUI != null)
            {
                if (TestPanelOrUI is Path || TestPanelOrUI is Ellipse || TestPanelOrUI is System.Windows.Shapes.Rectangle)
                {
                    if (TestPanelOrUI is Path) // if it's a curve
                    {
                        DrawCtrlPoints((Path)TestPanelOrUI);
                        DrawTngts((Path)TestPanelOrUI);
                        if (selectedPath == null)
                        {
                            selectedPath = (FrameworkElement)TestPanelOrUI;
                        }
                        else if (selectedPath.Equals((FrameworkElement)TestPanelOrUI) == false)
                        {
                            //removethe control point and the tangents for the las selected curve
                            foreach (Ellipse ell in shownPts)
                            {
                                canvas.Children.Remove(ell);
                            }
                            shownPts.Clear();

                            foreach (Shape ell in shownTngts)
                            {
                                canvas.Children.Remove(ell);
                            }
                            shownTngts.Clear();

                            selectedPath = (FrameworkElement)TestPanelOrUI;
                            //draw the caontrol points and the tangents for the new selected curve
                            DrawCtrlPoints((Path)TestPanelOrUI);
                            DrawTngts((Path)TestPanelOrUI);
                            ((Path)selectedPath).Stroke = Brushes.Black;
                        }
                        //add three functions to the curve
                        ((Path)TestPanelOrUI).MouseMove += new System.Windows.Input.MouseEventHandler(Path_MouseMove);
                        ((Path)TestPanelOrUI).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Path_MouseLeftButtonUp);
                        ((Path)TestPanelOrUI).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Path_MouseLeftButtonDown);
                    }
                    else if (TestPanelOrUI is Ellipse) // if it's a control point
                    {

                        if (selectedEllipse == null) selectedEllipse = (FrameworkElement)TestPanelOrUI;
                        else if (selectedEllipse.Equals((FrameworkElement)TestPanelOrUI) == false)
                        {
                            try { ((Ellipse)selectedEllipse).Stroke = Brushes.Red; }
                            catch (Exception) { }
                            selectedEllipse = (FrameworkElement)TestPanelOrUI;
                        }
                        try { ((Ellipse)selectedEllipse).Stroke = Brushes.Blue; }
                        catch (Exception) { }
                        //add three functions to the ellipse
                        ((Ellipse)TestPanelOrUI).MouseMove += new System.Windows.Input.MouseEventHandler(Ellipse_MouseMove);
                        ((Ellipse)TestPanelOrUI).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Ellipse_MouseLeftButtonUp);
                        ((Ellipse)TestPanelOrUI).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Ellipse_MouseLeftButtonDown);


                    }
                    else if (TestPanelOrUI is System.Windows.Shapes.Rectangle)//if it's a tangent
                    {
                        if (selectedRectangle == null) selectedRectangle = (FrameworkElement)TestPanelOrUI;
                        else if (selectedRectangle.Equals((FrameworkElement)TestPanelOrUI) == false)
                        {
                            try { ((System.Windows.Shapes.Rectangle)selectedRectangle).Stroke = Brushes.Yellow; }
                            catch (Exception) { }
                            selectedRectangle = (FrameworkElement)TestPanelOrUI;
                        }
                        try { ((Ellipse)selectedRectangle).Stroke = Brushes.Blue; }
                        catch (Exception) { }
                        ((System.Windows.Shapes.Rectangle)TestPanelOrUI).MouseMove += new System.Windows.Input.MouseEventHandler(Rectangle_MouseMove);
                        ((System.Windows.Shapes.Rectangle)TestPanelOrUI).MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(Rectangle_MouseLeftButtonUp);
                        ((System.Windows.Shapes.Rectangle)TestPanelOrUI).MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(Rectangle_MouseLeftButtonDown);
                    }
                }
                else
                {
                    foreach (Ellipse ell in shownPts)
                    {
                        canvas.Children.Remove(ell);
                    }
                    shownPts.Clear();
                    foreach (Shape ell in shownTngts)
                    {
                        canvas.Children.Remove(ell);
                    }
                    shownTngts.Clear();
                    selectedPath = null;
                    selectedEllipse = null;
                }
            }
        }
        private void MouseLeftDraw(MouseButtonEventArgs e)//when click on left button mouse in draw mode
        {
            currentCurveCtrlPts.Remove(currentCurveCtrlPts.Last());
            geoPath.Figures.Remove(temporaryFigure);
            DrawBezierCurve(true); //end the drawing of the curve
            //remove the control points from the canvas
            foreach (Ellipse ell in shownPts)
            {
                canvas.Children.Remove(ell);
            }
            shownPts.Clear();
            foreach (Shape ell in shownTngts)
            {
                canvas.Children.Remove(ell);
            }
            shownTngts.Clear();
        }

        private void MouseLeftModify(MouseButtonEventArgs e)// left mouse click on normal mode used to add a control point
        {
            object TestPanelOrUI = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (TestPanelOrUI != null)
            {
                if (TestPanelOrUI is Ellipse)
                {
                    if (firstEllipse == null)
                    {
                        firstEllipse = (Ellipse)TestPanelOrUI;
                        try { ((Ellipse)firstEllipse).Fill = Brushes.Green; }
                        catch (Exception) { }
                    }
                    else if (secondEllipse == null && !firstEllipse.Equals((Ellipse)TestPanelOrUI))
                    {
                        if ((shownPts.IndexOf(firstEllipse) != shownPts.IndexOf((Ellipse)TestPanelOrUI) + 1 && shownPts.IndexOf(firstEllipse) != shownPts.IndexOf((Ellipse)TestPanelOrUI) - 1))
                        {
                            try { ((Ellipse)secondEllipse).Fill = Brushes.Red; }
                            catch (Exception) { }
                            try { ((Ellipse)firstEllipse).Fill = Brushes.Red; }
                            catch (Exception) { }
                            secondEllipse = null;
                            firstEllipse = (Ellipse)TestPanelOrUI;
                            try { ((Ellipse)firstEllipse).Fill = Brushes.Green; }
                            catch (Exception) { }
                        }
                        else
                        {
                            secondEllipse = (Ellipse)TestPanelOrUI;
                            try { ((Ellipse)secondEllipse).Fill = Brushes.Green; }
                            catch (Exception) { }
                        }
                    }
                    else if (secondEllipse != null && !secondEllipse.Equals((Ellipse)TestPanelOrUI))
                    {
                        try { ((Ellipse)secondEllipse).Fill = Brushes.Red; }
                        catch (Exception) { }
                        try { ((Ellipse)firstEllipse).Fill = Brushes.Red; }
                        catch (Exception) { }
                        secondEllipse = null;
                        firstEllipse = (Ellipse)TestPanelOrUI;
                        try { ((Ellipse)firstEllipse).Fill = Brushes.Green; }
                        catch (Exception) { }
                    }
                }
            }
        }
        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {//handles click
            if (drawBtn.IsChecked == true)
            {
                MouseDownDraw(e);
            }
            else
            {
                MouseDownModify(e);
            }
            if (selectedPath != null)
            {
                showPathAltitudeOnTextBox((Path)selectedPath);
                showPathThicknessOnTextBox((Path)selectedPath);
            }
        }

        private void MouseMoveOnDraw(object sender, MouseEventArgs e)
        {
            if (currentCurveCtrlPts.Count != 0) //to handle real-time drawing
            {
                if (currentCurveCtrlPts.Last().Equals(mousePos))
                {
                    currentCurveCtrlPts.RemoveAt(currentCurveCtrlPts.Count - 1);
                }
                if (geoPath.Figures.Contains(temporaryFigure))
                {
                    geoPath.Figures.Remove(temporaryFigure);
                }
                mousePos = new Point(e.GetPosition(this.canvas).X, e.GetPosition(this.canvas).Y);
                currentCurveCtrlPts.Add(mousePos);
                DrawBezierCurve(false);
            }
        }

        private void MouseMoveOnCanvas(object sender, MouseEventArgs e)
        {//handles mouse moves
            MouseMoveOnDraw(sender, e);
        }

        private void Draw_Click(object sender, MouseButtonEventArgs e)
        {//handles right click
            if (currentCurveCtrlPts.Count != 0)
            {
                MouseLeftDraw(e);
            }
            else   //curve modification mode, we wil check which kid of element it is
            {
                MouseLeftModify(e);
            }
        }
        //Global informaion on the selected elements (of all kinds)
        Ellipse secondEllipse, firstEllipse;

        bool isDragging, mvCtrl = true;
        FrameworkElement elDragging, selectedPath;
        Point ptMouseStart, ptElementStart;
        double minX, minY, maxX, maxY;
        Thickness margin;

        //these  three functions handle the selection and the moving of the curve
        void Path_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            mvCtrl = true;
            ptMouseStart = e.GetPosition(this);
            elDragging = (this).InputHitTest(ptMouseStart) as FrameworkElement;
            if (elDragging == null) return;
            if (elDragging != null && elDragging is System.Windows.Shapes.Path)
            {
                ptElementStart = new Point(elDragging.Margin.Left, elDragging.Margin.Top);
                margin = new Thickness(elDragging.Margin.Left, elDragging.Margin.Top, 0, 0);
                ((Path)elDragging).Cursor = Cursors.ScrollAll;
                Mouse.Capture(((Path)elDragging));
                isDragging = true;
                maxX = MaxPtCtrlX((Path)elDragging) - ptMouseStart.X;
                maxY = MaxPtCtrlY((Path)elDragging) - ptMouseStart.Y;
                minX = -MinPtCtrlX((Path)elDragging) + ptMouseStart.X;
                minY = -MinPtCtrlY((Path)elDragging) + ptMouseStart.Y;
            }
        }

        void Path_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Point pnt = e.GetPosition(this);
            if (((Path)elDragging) == null) return;

            if (isDragging)
            {
                if (!mvCtrl)
                {
                    elDragging.Margin = margin;
                }
                int k = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {
                    if (r.Item2.Equals(((Path)elDragging).Data))
                    {
                        for (int i = 0; i < r.Item1.Count; i++)
                        {
                            r.Item1[i] = new Point(r.Item1[i].X + margin.Left - ptElementStart.X, r.Item1[i].Y + margin.Top - ptElementStart.Y);
                        }
                        for (int i = 0; i < r.Item3.Count; i++)
                        {
                            r.Item3[i] = new Point(r.Item3[i].X + margin.Left - ptElementStart.X, r.Item3[i].Y + margin.Top - ptElementStart.Y);
                        }
                        break;
                    }
                    k++;
                }
                //push the action to the undo stack "ctrl+z"
                actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("movePath", k, 0, 0, new Point(-margin.Left + ptElementStart.X, -margin.Top + ptElementStart.Y), new Point(0, 0), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                DrawCtrlPoints((Path)elDragging);
                DrawTngts((Path)elDragging);
            }
            isDragging = false;
            ((Path)elDragging).Cursor = Cursors.Arrow;
            ((Path)elDragging).ReleaseMouseCapture();
            elDragging = null;
        }

        void Path_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {//path dragging
            if (((Path)elDragging) == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point ptMouse = e.GetPosition(this);
                if (isDragging)
                {
                    foreach (Ellipse ell in shownPts)
                    {
                        canvas.Children.Remove(ell);
                    }
                    shownPts.Clear();
                    foreach (Shape ell in shownTngts)
                    {
                        canvas.Children.Remove(ell);
                    }
                    shownTngts.Clear();
                    if (elDragging == null)
                        elDragging = ((Path)elDragging);
                    double left = ptElementStart.X + ptMouse.X - ptMouseStart.X;
                    double top = ptElementStart.Y + ptMouse.Y - ptMouseStart.Y;
                    elDragging.Margin = new Thickness(left, top, 0, 0);// modify the margin to move the curve
                    if (mvCtrl)
                    {
                        margin = elDragging.Margin;
                    }
                    if (!(ptMouse.X + maxX <= 920 && ptMouse.X - minX >= 5 && ptMouse.Y - minY >= 5 && ptMouse.Y + maxY <= 525))
                    {
                        //in order to the curve stay in the canvas
                        if (ptMouse.X + maxX > 920 && mvCtrl)
                            margin.Left = margin.Left - ptMouse.X - maxX + 920;
                        if (ptMouse.X - minX < 5 && mvCtrl)
                            margin.Left = margin.Left + 5 - ptMouse.X + minX;
                        if (ptMouse.Y + maxY > 525 && mvCtrl)
                            margin.Top = margin.Top + 525 - ptMouse.Y - maxY;
                        if (ptMouse.Y - minY < 5 && mvCtrl)
                            margin.Top = margin.Top + 5 - ptMouse.Y + minY;
                        mvCtrl = false;
                    }

                }
            }
        }
        void DrawCtrlPoints(Path path)// draw control point of a curve
        {
            if (shownPts.Count != 0) return;
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(path.Data))
                {
                    foreach (Point p in r.Item1)
                    {
                        double ellipseSize = 10;
                        Ellipse ell = new Ellipse();
                        ell.Width = ellipseSize;
                        ell.Height = ellipseSize;
                        ell.Fill = Brushes.Red;
                        ell.Stroke = Brushes.Transparent;
                        Canvas.SetLeft(ell, p.X - ellipseSize / 2);
                        Canvas.SetTop(ell, p.Y - ellipseSize / 2);
                        shownPts.Add(ell);
                        canvas.Children.Add(ell);
                    }
                }
            }
        }
        private void Draw_Click(object sender, RoutedEventArgs e)
        {

        }
        private double MinPtCtrlX(Path pathSele)
        {//minimum X coordinate of a curve
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(pathSele.Data))
                {
                    double min = r.Item1[0].X;
                    foreach (Point p in r.Item1)
                    {
                        if (p.X < min)
                            min = p.X;  // is the new min
                    }
                    return min;
                }
            }
            return 0;
        }
        private double MinPtCtrlY(Path pathSele)
        {// min Y in a curve
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(pathSele.Data))
                {
                    double min = r.Item1[0].Y;
                    foreach (Point p in r.Item1)
                    {
                        if (p.Y < min)
                            min = p.Y;
                    }
                    return min;
                }
            }
            return 0;
        }

        private double MaxPtCtrlX(Path pathSele)
        {// max X in a curve
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(pathSele.Data))
                {
                    double max = r.Item1[0].X;
                    foreach (Point p in r.Item1)
                    {
                        if (p.X > max)
                            max = p.X;
                    }
                    return max;
                }
            }
            return 0;
        }

        private double MaxPtCtrlY(Path pathSele)
        {
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(pathSele.Data))
                {
                    double max = r.Item1[0].Y;
                    foreach (Point p in r.Item1)
                    {
                        if (p.Y > max)
                            max = p.Y;
                    }

                    return max;
                }
            }
            return 0;
        }
        /********               Ellipse Move *******************/
        bool isDraggingEllipse;
        FrameworkElement elDraggingEllipse, selectedEllipse;
        int IndPath, IndPnt;
        //these three functions handle the moving and the selection of a control point that is represented by ellipse
        void Ellipse_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            elDraggingEllipse = (this).InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (elDraggingEllipse == null) return;
            if (elDraggingEllipse != null && elDraggingEllipse is System.Windows.Shapes.Ellipse)
            {
                int i = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {
                    if (r.Item2.Equals(((Path)selectedPath).Data))   //search for the grometry path
                    {
                        IndPath = i;
                        break;
                    }
                    i++;
                }
                IndPnt = shownPts.IndexOf((Ellipse)selectedEllipse);
                //push the action to the undo stack "ctrl+z"
                actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("MovePoint", IndPath, IndPnt, 0, allCurvesData[IndPath].Item1[IndPnt], new Point(0, 0), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                ((Ellipse)elDraggingEllipse).Cursor = Cursors.ScrollAll;
                Mouse.Capture(((Ellipse)elDraggingEllipse));
                isDraggingEllipse = true;
            }
        }

        void Ellipse_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((Ellipse)elDraggingEllipse) == null) return;
            ((Ellipse)elDraggingEllipse).Cursor = Cursors.Arrow; //change the cursor
            ((Ellipse)elDraggingEllipse).ReleaseMouseCapture();
            isDraggingEllipse = false;
        }

        void Ellipse_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pnt;
            int i = 0;
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(((Path)selectedPath).Data))// search for the curve
                {
                    IndPath = i;
                }
                else
                    i++;
            }
            IndPnt = shownPts.IndexOf((Ellipse)selectedEllipse);

            if (((Ellipse)elDraggingEllipse) == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point ptMouse = e.GetPosition(this.canvas);
                if (ptMouse.X > 0 && ptMouse.X < canvas.Width && ptMouse.Y > 0 && ptMouse.Y < canvas.Height)
                {
                    if (isDraggingEllipse)
                    {
                        if (elDraggingEllipse == null)
                            elDraggingEllipse = ((Ellipse)elDraggingEllipse);
                        //change the position of the ellipse that represent the control point
                        Canvas.SetLeft(elDraggingEllipse, ptMouse.X  - 10 / 2);
                        Canvas.SetTop(elDraggingEllipse, ptMouse.Y   - 10 / 2);
                    }

                    pnt = new Point(e.GetPosition(this.canvas).X , e.GetPosition(this.canvas).Y );
                    //apply the changement in the list of allCurvesData
                    if (IndPnt != allCurvesData[IndPath].Item1.Count - 1)
                    {
                        allCurvesData[IndPath].Item3[2 * IndPnt] = new Point(allCurvesData[IndPath].Item3[2 * IndPnt].X + pnt.X - allCurvesData[IndPath].Item1[IndPnt].X, allCurvesData[IndPath].Item3[2 * IndPnt].Y + pnt.Y - allCurvesData[IndPath].Item1[IndPnt].Y);
                    }
                    if (IndPnt != 0)
                    {
                        allCurvesData[IndPath].Item3[2 * IndPnt - 1] = new Point(allCurvesData[IndPath].Item3[2 * IndPnt - 1].X + pnt.X - allCurvesData[IndPath].Item1[IndPnt].X, allCurvesData[IndPath].Item3[2 * IndPnt - 1].Y + pnt.Y - allCurvesData[IndPath].Item1[IndPnt].Y);
                    }
                    allCurvesData[IndPath].Item1[IndPnt] = pnt;
                    currentCurveCtrlPts = allCurvesData[IndPath].Item1;
                    DrawBezierCurve(currentCurveCtrlPts, allCurvesData[IndPath].Item3, true, false);
                    DrawTngts((Path)selectedPath);
                }
            }
        }
        /********               Rectangle Move *******************/
        bool isDraggingRectangle;
        FrameworkElement elDraggingRectangle, selectedRectangle;
        //these three functions in order to handle the moving and the selection of a rectangle that represent the tangents
        void Rectangle_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            elDraggingRectangle = (this).InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (elDraggingRectangle == null) return;
            if (elDraggingRectangle != null && elDraggingRectangle is System.Windows.Shapes.Rectangle)
            {
                ((System.Windows.Shapes.Rectangle)elDraggingRectangle).Cursor = Cursors.ScrollAll;
                Mouse.Capture(((System.Windows.Shapes.Rectangle)elDraggingRectangle));
                isDraggingRectangle = true;
                int i = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {
                    if (r.Item2.Equals(((Path)selectedPath).Data))//search the curve
                    {
                        IndPath = i;
                    }
                    else
                        i++;
                }
                i = 0;
                Ind = 0;
                foreach (Shape sh in shownTngts)
                {
                    if (sh.Equals((Shape)elDraggingRectangle))
                    {
                        IndPnt1 = i - Ind;//search for the index of the tangent that will be moved
                        if ((i - Ind - 1) % 2 != 0)
                        {
                            IndPnt2 = IndPnt1 - 1;// and the index of the other tangent that must be moved when the first tangent move 
                        }
                        else
                        {
                            IndPnt2 = IndPnt1 + 1;
                        }
                        break;
                    }
                    i++;
                    if (sh is Line) Ind++;// the index of the control point relatif to the tangent
                }
                if (IndPnt1 == allCurvesData[IndPath].Item3.Count - 1) IndPnt2 = -1;
                //push the action to the undo stack "ctrl+z"
                if (IndPnt2 != -1) actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", IndPath, IndPnt1, IndPnt2, allCurvesData[IndPath].Item3[IndPnt1], allCurvesData[IndPath].Item3[IndPnt2], (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                else actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", IndPath, IndPnt1, IndPnt2, allCurvesData[IndPath].Item3[IndPnt1], new Point(0, 0), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
            }

        }

        void Rectangle_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (((System.Windows.Shapes.Rectangle)elDraggingRectangle) == null) return;
            ((System.Windows.Shapes.Rectangle)elDraggingRectangle).Cursor = Cursors.Arrow;
            ((System.Windows.Shapes.Rectangle)elDraggingRectangle).ReleaseMouseCapture();
            isDraggingRectangle = false;
            elDraggingRectangle = null;
            DrawTngts((Path)selectedPath);
        }
        int IndPnt1, IndPnt2 = -1, Ind;
        void Rectangle_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Point pnt;
            if (((System.Windows.Shapes.Rectangle)elDraggingRectangle) == null) return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point ptMouse = e.GetPosition(this.canvas);
                if (ptMouse.X > 0 && ptMouse.X < canvas.Width && ptMouse.Y > 0 && ptMouse.Y < canvas.Height) {
                    if (isDraggingRectangle)
                    {
                        if (elDraggingRectangle == null)
                            elDraggingRectangle = ((System.Windows.Shapes.Rectangle)elDraggingRectangle);
                        //move the rectangle that represent the tangent
                        Canvas.SetLeft(elDraggingRectangle, ptMouse.X - 5 / 2);
                        Canvas.SetTop(elDraggingRectangle, ptMouse.Y - 5 / 2);
                    }
                    pnt = new Point(e.GetPosition(this.canvas).X, e.GetPosition(this.canvas).Y);

                    allCurvesData[IndPath].Item3[IndPnt1] = pnt;
                    double X1 = pnt.X;
                    double X2 = allCurvesData[IndPath].Item1[Ind].X;
                    double Y1 = pnt.Y;
                    double Y2 = allCurvesData[IndPath].Item1[Ind].Y;

                    if (IndPnt2 != -1)
                    {
                        double d1 = Distance(pnt, allCurvesData[IndPath].Item1[Ind]);
                        double d2 = Distance(allCurvesData[IndPath].Item3[IndPnt2], allCurvesData[IndPath].Item1[Ind]);
                        //calculate the position of the other tangent
                        Point pt2 = new Point(X2 + (X2 - X1) * (d2 / d1), Y2 + (Y2 - Y1) * (d2 / d1));
                        allCurvesData[IndPath].Item3[IndPnt2] = pt2;//change in allCurvesData
                        Y2 = pt2.Y;
                        X2 = pt2.X;
                        //move the other tanngent
                        Canvas.SetLeft(shownTngts[IndPnt2 + Ind], pt2.X - 5 / 2);
                        Canvas.SetTop(shownTngts[IndPnt2 + Ind], pt2.Y - 5 / 2);
                    }
                    canvas.Children.Remove(shownTngts[Ind + (int)Math.Max(IndPnt1, IndPnt2) + 1]);
                    Line l = new Line();
                    l.StrokeThickness = 1;
                    l.Fill = Brushes.Black;
                    l.Stroke = Brushes.Black;
                    l.X1 = X1;
                    l.X2 = X2;
                    l.Y1 = Y1;
                    l.Y2 = Y2;
                    canvas.Children.Add(l);
                    shownTngts[Ind + (int)Math.Max(IndPnt1, IndPnt2) + 1] = l;
                    //redraw the curve according to the new changes
                    DrawBezierCurve(allCurvesData[IndPath].Item1, allCurvesData[IndPath].Item3, true, false);
                }
            }
        }

        private void ThickSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {//change thickness
            if (selectedPath!=null)
            {
                ((Path)selectedPath).StrokeThickness = ThickSlider.Value;
            ThickText.Text = ThickSlider.Value.ToString();
            }
        }
        private void showPathThicknessOnTextBox(Path path)
        {//to be called when a path is selected
            ThickText.Text = path.StrokeThickness.ToString();
        }

        private void altitudeSliderValueChange(object sender, RoutedPropertyChangedEventArgs<double> e)
        {//change altitude
            changeSelectedCurveAltitude((float)AltSlider.Value);
        }

        List<Shape> shownTngts = new List<Shape>();

        void DrawTngts(Path path)//draw tangents of a curve
        {//show the tangents on the Canvas
            foreach (Shape ell in shownTngts)
            {
                canvas.Children.Remove(ell);

            }
            shownTngts.Clear();
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(path.Data))
                {
                    Line line = new Line();
                    line.StrokeThickness = 1;
                    line.Fill = Brushes.Black;
                    line.Stroke = Brushes.Black;
                    line.X1 = r.Item1.First().X;
                    line.Y1 = r.Item1.First().Y;
                    int i = 0;
                    foreach (Point p in r.Item3)
                    {
                        System.Windows.Shapes.Rectangle rect = new System.Windows.Shapes.Rectangle();
                        line.X2 = p.X;
                        line.Y2 = p.Y;
                        double rectangleSize = 5;
                        rect.Height = rectangleSize;
                        rect.Width = rectangleSize;
                        rect.Fill = Brushes.Yellow;
                        rect.Stroke = Brushes.Transparent;
                        Canvas.SetLeft(rect, p.X - rectangleSize / 2);
                        Canvas.SetTop(rect, p.Y - rectangleSize / 2);
                        shownTngts.Add(rect);
                        canvas.Children.Add(rect);
                        if (i % 2 == 0)
                        {
                            shownTngts.Add(line);
                            canvas.Children.Add(line);
                        }
                        i++;
                        line = new Line();
                        line.StrokeThickness = 1;
                        line.Fill = Brushes.Black;
                        line.Stroke = Brushes.Black;
                        line.X1 = p.X;
                        line.Y1 = p.Y;
                    }
                    line.X2 = r.Item1.Last().X;
                    line.Y2 = r.Item1.Last().Y;
                    shownTngts.Add(line);
                    canvas.Children.Add(line);
                }
            }

        }
        private void changeSelectedCurveAltitude(float altit)
        {//the real change
            if (selectedPath == null)
            {
                return;
            }
            Tuple<List<Point>, PathGeometry, List<Point>, float> tup;
            for (int i = 0; i < allCurvesData.Count; i++)
            {
                if (allCurvesData[i].Item2 == ((Path)selectedPath).Data)
                {//this only changes tha altitude's value for that tuple (which is allCurvesData[i])
                    tup = allCurvesData[i];
                    allCurvesData[i] = new Tuple<List<Point>, PathGeometry, List<Point>, float>(tup.Item1, tup.Item2, tup.Item3, altit);
                    ((Path)selectedPath).Stroke = new SolidColorBrush(altitudeToColor(altit));
                }
            }
        }
        void showPathAltitudeOnTextBox(Path path)
        {//to be called when a curve is selected
            for (int i = 0; i < allCurvesData.Count; i++)
            {
                if (allCurvesData[i].Item2 == path.Data)
                {
                    AltitudeBox.Text = allCurvesData[i].Item4.ToString();
                }
            }
        }
        public static Color altitudeToColor(float altit)
        {//implemeting the predefined altitude levels
            if (altit <= 0) return Colors.Black;
            if (altit < 50) return Colors.Cyan;
            if (altit < 100) return Colors.Blue;
            if (altit < 150) return Colors.Green;
            if (altit < 200) return Colors.Yellow;
            if (altit < 250) return Colors.Orange;
            return Colors.Red;
        }
        //-----------------------Delaunayyyyy !--------------------------------
        List<Polygon> delaunayPolygons = new List<Polygon>();

        private void DelaunayTr()
        {//Handles Delaunay Boutton Click
            if ((bool)Triang.IsChecked)
            {
                List<Point> list = new List<Point>();
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> tup in allCurvesData)
                {
                    foreach (Point p in tup.Item1)
                    {
                        list.Add(p);
                    }
                }
                triangulation(list);
            }
            else
            {
                removeDelaunayPolygons();
            }
        }

        private void triangulation(List<Point> points)
        {//Delaunay triangulation
            const double bigValue = 100000000;
            Point[] pointArr;
            Triangle triangle1, triangle2, triangle3;
            List<Triangle> triangles = new List<Triangle>();
            triangles.Add(new Triangle(new Point(-bigValue, -bigValue), new Point(-bigValue, bigValue), new Point(bigValue, 0)));
            foreach (Point point in points)
            {
                for (int i = 0; i < triangles.Count; i++)
                {// check which triangle holds the point
                    if (triangles[i].Includes(point)) 
                    {
                        pointArr = triangles[i].PointsAsArray();
                        triangles.RemoveAt(i);
                        triangle1 = new Triangle(point, pointArr[0], pointArr[1]);
                        triangle2 = new Triangle(point, pointArr[0], pointArr[2]);
                        triangle3 = new Triangle(point, pointArr[2], pointArr[1]);
                        triangles.Add(triangle1);
                        triangles.Add(triangle2);
                        triangles.Add(triangle3);
                        triangle1.Legalize(triangles);
                        triangle2.Legalize(triangles);
                        triangle3.Legalize(triangles);
                        break;
                    }
                }
            }

            for (int i = 0; i < triangles.Count; i++)
            {
                foreach (Point p in triangles[i].PointsAsArray())
                {
                    if ((Math.Abs(p.X) == bigValue) || (Math.Abs(p.Y) == bigValue))
                    {
                        triangles.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }
            RenderTriangulationPolygons(triangles);
        }
        private void RenderTriangulationPolygons(List<Triangle> triangles)
        {//render the triangles
            Polygon poly;
            foreach (Triangle t in triangles)
            {
                poly = new Polygon();
                poly.Points.Add(t.p1);
                poly.Points.Add(t.p2);
                poly.Points.Add(t.p3);
                poly.Stroke = Brushes.Black;
                delaunayPolygons.Add(poly);
                canvas.Children.Add(poly);
                triangles3D.Add(t);
            }
        }

        private void keyDownOnAltitude(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                try
                {
                    float altit = int.Parse(AltitudeBox.Text);
                    changeSelectedCurveAltitude(altit);
                }
                catch (System.FormatException)
                {
                    (new MssgBox("Enter a numeric value to the altitude !")).ShowDialog();
                }
                catch (System.NullReferenceException )
                {
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void removeDelaunayPolygons()
        {
            foreach (Polygon poly in delaunayPolygons)
            {
                canvas.Children.Remove(poly);
            }
            delaunayPolygons.Clear();
        }

        public void LoadThings(string path)
        {//Load a saved file from a path
            try
            {
                SaveFormat fileData;
                XmlSerializer xmlSeries = new XmlSerializer(typeof(SaveFormat));
                using (System.IO.TextReader reader = new System.IO.StreamReader(path))
                {
                    fileData = (SaveFormat)xmlSeries.Deserialize(reader);
                }
                if ((fileData.user != "") && (fileData.user != Login.currentUser))
                {
                    (new MssgBox("You can't access that file !")).ShowDialog();
                    return;
                }
                allCurvesData.Clear();
                actions.Clear();
                actionsRedo.Clear();
                canvas.Children.Clear();
                canvas.Children.Add(im);
                im.Source = null;
                canvas.Children.Add(this.path);
                for (int i = 0; i < fileData.l1.Count; i++)
                {
                    Path thisPath = DrawBezierCurve(fileData.l1[i], fileData.l3[i], true, true, fileData.l4[i]);
                    thisPath.StrokeThickness = fileData.l5[i];
                    thisPath.Stroke = new SolidColorBrush(altitudeToColor(fileData.l4[i]));
                }
                (new MssgBox("Loaded successfully !")).ShowDialog();
            }
            catch (Exception)
            {
                new MssgBox("Impossible to load this file !").ShowDialog();
            }
        }

        public void SaveThings(string path)
        {//save to a file
            List<List<Point>> l1 = new List<List<Point>>();
            List<Thickness> l2 = new List<Thickness>();
            List<List<Point>> l3 = new List<List<Point>>();
            List<float> l4 = new List<float>();
            List<double> l5 = new List<double>();
            Thickness margin = new Thickness();
            double thickness = 0;
            XmlSerializer xmlSeries = new XmlSerializer(typeof(SaveFormat));

            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> el in allCurvesData)
            {
                foreach (Object p in canvas.Children)
                {
                    if (p is Path)
                    {
                        if (((Path)p).Data.Equals(el.Item2))
                        {
                            margin = ((Path)p).Margin;
                            thickness = ((Path)p).StrokeThickness;
                        }
                    }
                }
                l1.Add(el.Item1);
                l3.Add(el.Item3);
                l4.Add(el.Item4);
                l5.Add(thickness);
            }
            using (System.IO.TextWriter writer = new System.IO.StreamWriter(path))
            {
                xmlSeries.Serialize(writer, new SaveFormat(l1, l2, l3, l4,l5, Login.currentUser));
            }
            (new MssgBox("Saved successfully !")).ShowDialog();
        }

        #region Interface

        #region ZoomButtons
        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {//Zoom in 
            ZoomSlider.Value+=0.25;
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {//Zoom out
            ZoomSlider.Value-=0.25;
        }
        #endregion
        #region Navigation
        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService.Navigate(new MenuPage(im,canvas));
        }
        #endregion
        #region Togglebuttons act as radiobuttons
        private void Menu_event(object sender, RoutedEventArgs e)
        {
            Login.Logout();
            (new MssgBox("You have logged out !")).ShowDialog();
            this.NavigationService.Navigate(new MainPage());
            // this.NavigationService.Navigate(new MenuPage(canvas));

        }
        private void drawBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (Ellipse ell in shownPts) canvas.Children.Remove(ell);
            foreach (Shape ell in shownTngts) canvas.Children.Remove(ell);
            shownPts.Clear();
            shownTngts.Clear();
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void Triang_Click(object sender, RoutedEventArgs e)
        {
            DelaunayTr();
        }

        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //Panel panel = _ImageScrollArea;
            Canvas panel = canvas;

            // Make sure the control's are all ready.
            if (!IsInitialized) return;

            //Set the scale coordinates on the ScaleTransform from the slider
            double scale = (double)_Slider.Value;
            canvas.LayoutTransform = new ScaleTransform(scale, scale);

            //Set the zoom (this will affect rotate too) origin to the center of the panel
            panel.RenderTransformOrigin = new Point(0, 0);
            

        }

        

        private void _3D_Click(object sender, RoutedEventArgs e)
        {//shows the 3D window 
            float alt1 = 0, alt2 = 0, alt3 = 0;
            List<Point3D> listPoint3D = new List<Point3D>();

            foreach (Triangle t in triangles3D)
            {
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> tup in allCurvesData)
                {
                    if (tup.Item1.Contains(t.p1))
                    {
                        alt1 = tup.Item4;
                    }
                    if (tup.Item1.Contains(t.p2))
                    {
                        alt2 = tup.Item4;
                    }
                    if (tup.Item1.Contains(t.p3))
                    {
                        alt3 = tup.Item4;
                    }
                }
                listPoint3D.Add(new Point3D(t.p1.X - 464, alt1, t.p1.Y - 209));
                listPoint3D.Add(new Point3D(t.p2.X - 464, alt2, t.p2.Y - 209));
                listPoint3D.Add(new Point3D(t.p3.X - 464, alt3, t.p3.Y - 209));
            }
            MeshGeometry3D mesh = new MeshGeometry3D();
            for (int i = 0; i < listPoint3D.Count; i++)
            {
                mesh.Positions.Add(listPoint3D[i]);
            }
            List<Int32> indices = new List<Int32>();
            for (int i = 0; i < listPoint3D.Count; i++)
            {
                indices.Add(i);
            }
            for (int i = 0; i < indices.Count; i++)
            {
                mesh.TriangleIndices.Add(indices[i]);
            }
            GeometryModel3D shape = new GeometryModel3D();
            shape.Geometry = mesh;
            shape.Material = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            shape.BackMaterial = new DiffuseMaterial(new SolidColorBrush(Colors.Gray));
            DirectionalLight DirLight1 = new DirectionalLight();
            DirLight1.Color = Colors.White;
            DirLight1.Direction = new Vector3D(-2, -2, -6);
            DirectionalLight DirLight2 = new DirectionalLight();
            DirLight2.Color = Colors.White;
            DirLight2.Direction = new Vector3D(2, -2, 6);
            PerspectiveCamera Camera1 = new PerspectiveCamera();
            // Camera1.FarPlaneDistance = 20;
            // Camera1.NearPlaneDistance = -50;
            Camera1.FieldOfView = 45;
            Camera1.Position = new Point3D(2, 2, 6);
            Camera1.LookDirection = new Vector3D(-2, -2, -6);
            Camera1.UpDirection = new Vector3D(0, 1, 0);


            Model3DGroup modelGroup = new Model3DGroup();
            modelGroup.Children.Add(shape);
            modelGroup.Children.Add(DirLight1);
            modelGroup.Children.Add(DirLight2);

            modelGroup.Transform = new ScaleTransform3D(0.007, 0.007, 0.007);

            ModelVisual3D modelVisual = new ModelVisual3D();
            modelVisual.Content = modelGroup;

            Viewport3D myViewport = new Viewport3D();
            myViewport.Camera = Camera1;
            myViewport.Children.Add(modelVisual);

            Window3D windwDel = new Window3D();

            windwDel.ShowActivated = true;
            foreach (Window win in Application.Current.Windows)
            {
                if (win.Name == "DWindow")
                {
                    ((Window3D)win).Delgrid.Children.Add(myViewport);
                }
            }
            windwDel.ShowDialog();
            _3D.IsChecked = false;
        }

        public static implicit operator AppilicationPage(MainPage v)
        {
            throw new NotImplementedException();
        }




        #endregion
        #region UserButton

        private void user_Click(object sender, RoutedEventArgs e)
        {
            Login login = new Login(guest, user, userin);
            login.ShowDialog();
        }
        #endregion
        #endregion
        private void Delete(object sender, RoutedEventArgs e)
        {//curve deletion
            if (selectedEllipse != null)
            {
                int i = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {

                    if (r.Item2.Equals(((Path)selectedPath).Data))
                    {
                        IndPath = i;
                        break;
                    }
                    else
                        i++;
                }
                IndPnt = shownPts.IndexOf((Ellipse)selectedEllipse);
                foreach (Ellipse ell in shownPts) canvas.Children.Remove(ell);
                foreach (Shape ell in shownTngts) canvas.Children.Remove(ell);
                shownPts.Clear();
                shownTngts.Clear();
                List<Point> c = CloneList(allCurvesData[IndPath].Item1), t = CloneList(allCurvesData[IndPath].Item3);
                altitudee = allCurvesData[IndPath].Item4;
                Path pth = ClonePath((Path)selectedPath);
                //push the action to the undo stack "ctrl+z"
                actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("deletePoint", IndPath, IndPnt, 0, new Point(), new Point(), pth, new Tuple<List<Point>, List<Point>, float>(c, t, altitudee)));
                try
                {
                    allCurvesData[IndPath].Item1.RemoveAt(IndPnt);
                }
                catch (Exception)
                {

                }
                if (allCurvesData[IndPath].Item1.Count != 1)
                {
                    currentCurveCtrlPts = allCurvesData[IndPath].Item1;
                    DrawBezierCurve(true, false, altitudee);
                    DrawTngts((Path)selectedPath);
                    DrawCtrlPoints((Path)selectedPath);
                }
                else
                {
                    //push the action to the undo stack "ctrl+z"
                    actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("delete", IndPath, 0, 0, new Point(), new Point(), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(allCurvesData[IndPath].Item1, allCurvesData[IndPath].Item3, allCurvesData[IndPath].Item4)));
                    canvas.Children.Remove(selectedPath);
                    allCurvesData.RemoveAt(IndPath);
                    selectedPath = null;
                }
                selectedEllipse = null;
            }
            else if (selectedPath != null)
            {
                foreach (Ellipse ell in shownPts) canvas.Children.Remove(ell);
                foreach (Shape ell in shownTngts) canvas.Children.Remove(ell);
                shownPts.Clear();
                shownTngts.Clear();
                int p = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {

                    if (r.Item2.Equals(((Path)selectedPath).Data))
                    {
                        break;
                    }
                    p++;
                }
                //push the action to the undo stack "ctrl+z"
                actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("delete", p, 0, 0, new Point(), new Point(), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(allCurvesData[p].Item1, allCurvesData[p].Item3, allCurvesData[p].Item4)));
                canvas.Children.Remove(selectedPath);
                try
                {
                    allCurvesData.RemoveAt(p);
                }
                catch (Exception) { }
                selectedPath = null;
            }
        }
        Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>> action;
        private void Undo(object sender, RoutedEventArgs e)
        {//undo any type of user action
            //using a stack where we stack a tuple that contain a string that represent the type of the action and other data needed in the Undo
            Path undoPath = new Path();
            if (drawBtn.IsChecked == true)
            {
                //undo the drawing of a control point while being in drawing mode
                try
                {
                    currentCurveCtrlPts.RemoveAt(currentCurveCtrlPts.Count - 2);
                    canvas.Children.Remove(shownPts.Last());
                    shownPts.Remove(shownPts.Last());
                    geoPath.Figures.Remove(temporaryFigure);
                    DrawBezierCurve(false);
                }
                catch (Exception)
                {

                }
            }
            if (actions.Count != 0)
            {
                action = actions.Pop();
                switch (action.Item1)
                {
                    case "MovePoint":
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("MovePoint", action.Item2, action.Item3, 0, allCurvesData[action.Item2].Item1[action.Item3], new Point(0, 0), action.Item7, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                        undoPath = action.Item7;
                        double dx, dy;
                        dx = action.Item5.X - allCurvesData[action.Item2].Item1[action.Item3].X;
                        dy = action.Item5.Y - allCurvesData[action.Item2].Item1[action.Item3].Y;
                        if (action.Item3 != allCurvesData[action.Item2].Item1.Count - 1)
                        {
                            allCurvesData[action.Item2].Item3[2 * action.Item3] = new Point(allCurvesData[action.Item2].Item3[2 * action.Item3].X + dx, allCurvesData[action.Item2].Item3[2 * action.Item3].Y + dy);
                        }
                        if (action.Item3 != 0)
                        {
                            allCurvesData[action.Item2].Item3[2 * action.Item3 - 1] = new Point(allCurvesData[action.Item2].Item3[2 * action.Item3 - 1].X + dx, allCurvesData[action.Item2].Item3[2 * action.Item3 - 1].Y + dy);
                        }
                        allCurvesData[action.Item2].Item1[action.Item3] = action.Item5;
                        currentCurveCtrlPts = allCurvesData[action.Item2].Item1;
                        selectedPath = undoPath;
                        IndPath = action.Item2;
                        altitudee = allCurvesData[IndPath].Item4;
                        DrawBezierCurve(currentCurveCtrlPts, allCurvesData[action.Item2].Item3, true, false,altitudee);

                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        DrawTngts(undoPath);
                        DrawCtrlPoints(undoPath);
                        break;
                    case "movePath":
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("movePath", action.Item2, 0, 0, new Point(-action.Item5.X, -action.Item5.Y), action.Item6, action.Item7, action.Rest));
                        undoPath = action.Item7;
                        ptElementStart = action.Item5;
                        undoPath.Margin = new Thickness(undoPath.Margin.Left + ptElementStart.X, undoPath.Margin.Top + ptElementStart.Y, 0, 0);
                        foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                        {
                            if (r.Item2.Equals((undoPath).Data))
                            {
                                for (int i = 0; i < r.Item1.Count; i++)
                                {
                                    r.Item1[i] = new Point(r.Item1[i].X + ptElementStart.X, r.Item1[i].Y + ptElementStart.Y);
                                }
                                for (int i = 0; i < r.Item3.Count; i++)
                                {
                                    r.Item3[i] = new Point(r.Item3[i].X + ptElementStart.X, r.Item3[i].Y + ptElementStart.Y);
                                }
                                break;
                            }

                        }
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        break;

                    case "moveTngnt":
                        if (action.Item4 != -1) actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", action.Item2, action.Item3, action.Item4, allCurvesData[action.Item2].Item3[action.Item3], allCurvesData[action.Item2].Item3[action.Item4], action.Item7, action.Rest));
                        else actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", action.Item2, action.Item3, action.Item4, allCurvesData[action.Item2].Item3[action.Item3], new Point(), action.Item7, action.Rest));
                        undoPath = action.Item7;
                        allCurvesData[action.Item2].Item3[action.Item3] = action.Item5;
                        if (action.Item4 != -1) allCurvesData[action.Item2].Item3[action.Item4] = action.Item6;
                        currentCurveCtrlPts = allCurvesData[action.Item2].Item1;
                        selectedPath = undoPath;
                        IndPath = action.Item2;
                        altitudee = allCurvesData[IndPath].Item4;
                        DrawBezierCurve(currentCurveCtrlPts, allCurvesData[action.Item2].Item3, true, false,altitudee);
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        DrawTngts(undoPath);
                        DrawCtrlPoints(undoPath);
                        break;
                    case "draw":
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("delete", action.Item2, 0, 0, action.Item5, action.Item6, action.Item7, new Tuple<List<Point>, List<Point>, float>(allCurvesData[action.Item2].Item1, allCurvesData[action.Item2].Item3, allCurvesData[action.Item2].Item4)));
                        undoPath = action.Item7;
                        canvas.Children.Remove(undoPath);
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        try
                        {
                            allCurvesData.RemoveAt(action.Item2);
                        }
                        catch (Exception)
                        {

                        }
                        break;
                    case "delete":
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("draw", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, action.Item7, action.Rest));
                        undoPath = action.Item7;
                        canvas.Children.Add(undoPath);
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        DrawCtrlPoints(undoPath);
                        DrawTngts(undoPath);
                        break;
                    case "addPoint":
                        foreach (UIElement r in canvas.Children)
                        {
                            if (r is Path)
                            {
                                if ((((Path)r).Data).Equals(allCurvesData[action.Item2].Item2))
                                {
                                    undoPath = (Path)r;
                                }
                            }
                        }
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("addPoint", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, ClonePath(undoPath), new Tuple<List<Point>, List<Point>, float>(CloneList(allCurvesData[action.Item2].Item1), CloneList(allCurvesData[action.Item2].Item3), allCurvesData[action.Item2].Item4)));
                        allCurvesData.RemoveAt(action.Item2);
                        undoPath.Data = (action.Item7).Data;
                        undoPath.Margin = (action.Item7).Margin;
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        DrawCtrlPoints(undoPath);
                        DrawTngts(undoPath);
                        break;
                    case "deletePoint":
                        foreach (UIElement r in canvas.Children)
                        {
                            if (r is Path)
                            {
                                if ((((Path)r).Data).Equals(allCurvesData[action.Item2].Item2))
                                {
                                    undoPath = (Path)r;
                                }
                            }
                        }
                        actionsRedo.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("deletePoint", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, ClonePath(undoPath), new Tuple<List<Point>, List<Point>, float>(CloneList(allCurvesData[action.Item2].Item1), CloneList(allCurvesData[action.Item2].Item3), allCurvesData[action.Item2].Item4)));

                        allCurvesData.RemoveAt(action.Item2);
                        undoPath.Data = (action.Item7).Data;
                        undoPath.Margin = (action.Item7).Margin;
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        DrawCtrlPoints(undoPath);
                        DrawTngts(undoPath);
                        break;

                }
            }
        }
        List<Point> CloneList(List<Point> c)
        {
            List<Point> t = new List<Point>();
            foreach (Point point in c) t.Add(point);
            return t;
        }
        Path ClonePath(Path selectedPath)
        {//used in copy/paste
            if (selectedPath == null) return null;
            Path pth = new Path();
            pth.Data = (PathGeometry)(((Path)selectedPath).Data).Clone();
            pth.Stroke = ((Path)selectedPath).Stroke;
            pth.StrokeThickness = ((Path)selectedPath).StrokeThickness;
            pth.Fill = ((Path)selectedPath).Fill;
            pth.Margin = ((Path)selectedPath).Margin;
            return pth;
        }

        private void Redo(object sender, RoutedEventArgs e)
        {//Redo the last undone action
            //use a stack also of an actions that contatin the undo actions
            Path undoPath = new Path();
            if (actionsRedo.Count != 0)
            {
                action = actionsRedo.Pop();
                switch (action.Item1)
                {
                    case "MovePoint":
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("MovePoint", action.Item2, action.Item3, 0, allCurvesData[action.Item2].Item1[action.Item3], new Point(0, 0), action.Item7, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
                        undoPath = action.Item7;
                        double dx, dy;
                        dx = action.Item5.X - allCurvesData[action.Item2].Item1[action.Item3].X;
                        dy = action.Item5.Y - allCurvesData[action.Item2].Item1[action.Item3].Y;
                        if (action.Item3 != allCurvesData[action.Item2].Item1.Count - 1)
                        {
                            allCurvesData[action.Item2].Item3[2 * action.Item3] = new Point(allCurvesData[action.Item2].Item3[2 * action.Item3].X + dx, allCurvesData[action.Item2].Item3[2 * action.Item3].Y + dy);
                        }
                        if (action.Item3 != 0)
                        {
                            allCurvesData[action.Item2].Item3[2 * action.Item3 - 1] = new Point(allCurvesData[action.Item2].Item3[2 * action.Item3 - 1].X + dx, allCurvesData[action.Item2].Item3[2 * action.Item3 - 1].Y + dy);
                        }
                        allCurvesData[action.Item2].Item1[action.Item3] = action.Item5;
                        currentCurveCtrlPts = allCurvesData[action.Item2].Item1;
                        selectedPath = undoPath;
                        IndPath = action.Item2;
                        altitudee = allCurvesData[IndPath].Item4;
                        DrawBezierCurve(currentCurveCtrlPts, allCurvesData[action.Item2].Item3, true, false, altitudee);

                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        DrawTngts(undoPath);
                        DrawCtrlPoints(undoPath);
                        break;
                    case "movePath":
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("movePath", action.Item2, 0, 0, new Point(-action.Item5.X, -action.Item5.Y), action.Item6, action.Item7, action.Rest));
                        undoPath = action.Item7;
                        ptElementStart = action.Item5;
                        undoPath.Margin = new Thickness(undoPath.Margin.Left + ptElementStart.X, undoPath.Margin.Top + ptElementStart.Y, 0, 0);
                        foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                        {
                            if (r.Item2.Equals((undoPath).Data))
                            {
                                for (int i = 0; i < r.Item1.Count; i++)
                                {
                                    r.Item1[i] = new Point(r.Item1[i].X + ptElementStart.X, r.Item1[i].Y + ptElementStart.Y);
                                }
                                for (int i = 0; i < r.Item3.Count; i++)
                                {
                                    r.Item3[i] = new Point(r.Item3[i].X + ptElementStart.X, r.Item3[i].Y + ptElementStart.Y);
                                }
                                break;
                            }

                        }
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        break;

                    case "moveTngnt":
                        if (action.Item4 != -1) actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", action.Item2, action.Item3, action.Item4, allCurvesData[action.Item2].Item3[action.Item3], allCurvesData[action.Item2].Item3[action.Item4], action.Item7, action.Rest));
                        else actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("moveTngnt", action.Item2, action.Item3, action.Item4, allCurvesData[action.Item2].Item3[action.Item3], new Point(), action.Item7, action.Rest));
                        undoPath = action.Item7;
                        allCurvesData[action.Item2].Item3[action.Item3] = action.Item5;
                        if (action.Item4 != -1) allCurvesData[action.Item2].Item3[action.Item4] = action.Item6;
                        currentCurveCtrlPts = allCurvesData[action.Item2].Item1;
                        selectedPath = undoPath;
                        IndPath = action.Item2;
                        altitudee = allCurvesData[IndPath].Item4;
                        DrawBezierCurve(currentCurveCtrlPts, allCurvesData[action.Item2].Item3, true, false, altitudee);
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        DrawTngts(undoPath);
                        DrawCtrlPoints(undoPath);
                        break;
                    case "delete":
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("draw", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, action.Item7, action.Rest));
                        undoPath = action.Item7;
                        canvas.Children.Add(undoPath);
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        break;
                    case "draw":
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("delete", action.Item2, 0, 0, action.Item5, action.Item6, action.Item7, new Tuple<List<Point>, List<Point>, float>(allCurvesData[action.Item2].Item1, allCurvesData[action.Item2].Item3, allCurvesData[action.Item2].Item4)));
                        undoPath = action.Item7;
                        canvas.Children.Remove(undoPath);
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        try
                        {
                            allCurvesData.RemoveAt(action.Item2);
                        }
                        catch (Exception)
                        {

                        }
                        break;
                    case "deletePoint":
                        foreach (UIElement r in canvas.Children)
                        {
                            if (r is Path)
                            {
                                if ((((Path)r).Data).Equals(allCurvesData[action.Item2].Item2))
                                {
                                    undoPath = (Path)r;
                                }
                            }
                        }
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("deletePoint", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, ClonePath(undoPath), new Tuple<List<Point>, List<Point>, float>(CloneList(allCurvesData[action.Item2].Item1), CloneList(allCurvesData[action.Item2].Item3), allCurvesData[action.Item2].Item4)));
                        allCurvesData.RemoveAt(action.Item2);
                        undoPath.Data = (action.Item7).Data;
                        undoPath.Margin = (action.Item7).Margin;
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        DrawCtrlPoints(undoPath);
                        DrawTngts(undoPath);
                        break;
                    case "addPoint":
                        foreach (UIElement r in canvas.Children)
                        {
                            if (r is Path)
                            {
                                if ((((Path)r).Data).Equals(allCurvesData[action.Item2].Item2))
                                {
                                    undoPath = (Path)r;
                                }
                            }
                        }
                        actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("addPoint", action.Item2, action.Item3, action.Item4, action.Item5, action.Item6, ClonePath(undoPath), new Tuple<List<Point>, List<Point>, float>(CloneList(allCurvesData[action.Item2].Item1), CloneList(allCurvesData[action.Item2].Item3), allCurvesData[action.Item2].Item4)));
                        allCurvesData.RemoveAt(action.Item2);
                        undoPath.Data = (action.Item7).Data;
                        undoPath.Margin = (action.Item7).Margin;
                        allCurvesData.Insert(action.Item2, new Tuple<List<Point>, PathGeometry, List<Point>, float>(action.Rest.Item1, (PathGeometry)(undoPath.Data), action.Rest.Item2, action.Rest.Item3));
                        foreach (Ellipse r in shownPts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownPts.Clear();
                        foreach (Shape r in shownTngts)
                        {
                            canvas.Children.Remove(r);
                        }
                        shownTngts.Clear();
                        DrawCtrlPoints(undoPath);
                        DrawTngts(undoPath);
                        break;
                }
            }
        }
        Path copiedPath;

        private void Copy(object sender, RoutedEventArgs e)
        {
            if (selectedPath == null) return;
            cut = false;
            copiedPath = (Path)selectedPath;
        }

        int cuttedIndex;
        List<Point> cuttedCntrlList, cuttedTangentList;
        float cuttedAltitude;
        bool cut;
        private void Paste(object sender, RoutedEventArgs e)
        {
            if (copiedPath == null) return;
            int k;
            List<Point> c = new List<Point>();
            List<Point> t = new List<Point>();
            float s;
            if (!cut)
            {
                k = 0;
                foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
                {
                    if (r.Item2.Equals(((Path)selectedPath).Data))
                    {
                        break;
                    }
                    k++;
                }
                int i = 0;
                foreach (Point point in allCurvesData[k].Item1)
                {
                    c.Add(point);
                    i++;
                }

                i = 0;
                foreach (Point point in allCurvesData[k].Item3)
                {
                    t.Add(point);
                    i++;
                }
                s = allCurvesData[k].Item4;
            }
            else
            {
                int i = 0;
                foreach (Point point in cuttedCntrlList)
                {
                    c.Add(point);
                    i++;
                }

                i = 0;
                foreach (Point point in cuttedTangentList)
                {
                    t.Add(point);
                    i++;
                }
                k = cuttedIndex;
                s = cuttedAltitude;

            }
            Path p = new Path();
            p.StrokeThickness = copiedPath.StrokeThickness;
            p.Stroke = copiedPath.Stroke;
            p.Data = (PathGeometry)(copiedPath.Data).Clone();
            p.Margin = copiedPath.Margin;
            canvas.Children.Add(p);
            allCurvesData.Add(new Tuple<List<Point>, PathGeometry, List<Point>, float>(c, (PathGeometry)(p.Data), t, s));
            actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("draw", allCurvesData.Count - 1, 0, 0, new Point(0, 0), new Point(0, 0), p, new Tuple<List<Point>, List<Point>, float>(null, null, 0.0f)));
        }

        private void Cut(object sender, RoutedEventArgs e)
        {//cut a path , removes it from the canvas white keeping it as a copied path
            if (selectedPath == null) return;
            int p = 0;
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (r.Item2.Equals(((Path)selectedPath).Data))
                {
                    break;
                }
                p++;
            }
            cuttedIndex = p;
            cuttedCntrlList = allCurvesData[p].Item1;
            cuttedTangentList = allCurvesData[p].Item3;
            cuttedAltitude = allCurvesData[p].Item4;
            cut = true;
            actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("delete", p, 0, 0, new Point(), new Point(), (Path)selectedPath, new Tuple<List<Point>, List<Point>, float>(allCurvesData[p].Item1, allCurvesData[p].Item3, allCurvesData[p].Item4)));
            copiedPath = (Path)selectedPath;
            foreach (Ellipse r in shownPts)
            {
                canvas.Children.Remove(r);
            }
            shownPts.Clear();
            foreach (Shape r in shownTngts)
            {
                canvas.Children.Remove(r);
            }
            shownTngts.Clear();
            allCurvesData.RemoveAt(p);

            canvas.Children.Remove(selectedPath);
            selectedPath = null;
        }

        private void Add(object sender, RoutedEventArgs e)
        {//add a control point
            if (firstEllipse == null) return;
            if (shownPts.Count == 0) return;
            int k = 0;
            foreach (Tuple<List<Point>, PathGeometry, List<Point>, float> r in allCurvesData)
            {
                if (((Path)selectedPath).Data == r.Item2) break;
                k++;
            }
            int index;
            if (secondEllipse == null) index = shownPts.IndexOf(firstEllipse);
            else index = Math.Min(shownPts.IndexOf(firstEllipse), shownPts.IndexOf(secondEllipse));
            index++;
            actions.Push(new Tuple<String, int, int, int, Point, Point, Path, Tuple<List<Point>, List<Point>, float>>("addPoint", k, index, 0, new Point(0, 0), new Point(0, 0), ClonePath((Path)selectedPath), new Tuple<List<Point>, List<Point>, float>(CloneList(allCurvesData[k].Item1), CloneList(allCurvesData[k].Item3), allCurvesData[k].Item4)));
            if (index == allCurvesData[k].Item1.Count) index--;
            allCurvesData[k].Item1.Insert(index, allCurvesData[k].Item1[index]);
            if (index + 1 != allCurvesData[k].Item1.Count)
            {
                if (index == 1 || index == 0) index = 2;
                allCurvesData[k].Item3.Insert(2 * index, allCurvesData[k].Item3[2 * (index - 1)]);
                allCurvesData[k].Item3.Insert(2 * index - 1, allCurvesData[k].Item3[2 * (index - 1) - 1]);
            }
            else
            {
                allCurvesData[k].Item3.Add(allCurvesData[k].Item3[2 * (index - 1) - 1]);
            }
            currentCurveCtrlPts = allCurvesData[k].Item1;
            IndPath = k;
            altitudee = allCurvesData[IndPath].Item4;
            DrawBezierCurve(true, false, altitudee);
            foreach (Ellipse ell in shownPts)
            {
                canvas.Children.Remove(ell);
            }
            shownPts.Clear();
            DrawTngts((Path)selectedPath);
            DrawCtrlPoints((Path)selectedPath);

        }

    }

}
class Triangle
{// a class that handles all the triangulatio subfunctions
    public Point p1, p2, p3;    /// <summary>
    //the points
    public Triangle(Point p, Point q, Point r)
    {
        p1 = p;
        p2 = q;
        p3 = r;
    }
    public bool Includes(Point p)
    {//check if a point is inside the triangle using angles
        double angle1, angle2, angle3;
        Vector vect1, vect2, vect3;
        vect1 = new Vector(p1.X - p.X, p1.Y - p.Y);
        vect2 = new Vector(p2.X - p.X, p2.Y - p.Y);
        vect3 = new Vector(p3.X - p.X, p3.Y - p.Y);
        angle1 = Math.Abs(Vector.AngleBetween(vect1, vect2));
        angle2 = Math.Abs(Vector.AngleBetween(vect2, vect3));
        angle3 = Math.Abs(Vector.AngleBetween(vect3, vect1));
        return (Math.Abs(angle1 + angle2 + angle3 - 360) < 0.1);
    }
    public Point[] PointsAsArray()
    {//returns the points as an array
        Point[] arr = new Point[3];
        arr[0] = p1; arr[1] = p2; arr[2] = p3;
        return arr;
    }
    public void Legalize(List<Triangle> triangles)
    {//Legalization (Recursive)
        List<Point> commonPts = new List<Point>();
        double angle1, angle2;
        Vector vect1, vect2;
        Point nonCommonPt1 = new Point(), nonCommonPt2 = new Point();
        Point[] array;
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i] == this)
            {
                continue;
            }
            array = triangles[i].PointsAsArray();
            foreach (Point pt in array)
            {
                if (PointsAsArray().Contains(pt))
                {
                    commonPts.Add(pt);
                }
                else
                {
                    nonCommonPt1 = pt;
                }

            }
            if (commonPts.Count == 2)
            {
                foreach (Point pt in PointsAsArray())
                {
                    if (!array.Contains(pt))
                    {
                        nonCommonPt2 = pt;
                    }
                }
                vect1 = new Vector(commonPts[0].X - nonCommonPt1.X, commonPts[0].Y - nonCommonPt1.Y);
                vect2 = new Vector(commonPts[1].X - nonCommonPt1.X, commonPts[1].Y - nonCommonPt1.Y);
                angle1 = Math.Abs(Vector.AngleBetween(vect1, vect2));
                vect1 = new Vector(commonPts[0].X - nonCommonPt2.X, commonPts[0].Y - nonCommonPt2.Y);
                vect2 = new Vector(commonPts[1].X - nonCommonPt2.X, commonPts[1].Y - nonCommonPt2.Y);
                angle2 = Math.Abs(Vector.AngleBetween(vect1, vect2));
                if (angle1 + angle2 > 180)
                {
                    Triangle trian1 = new Triangle(nonCommonPt1, nonCommonPt2, commonPts[0]);
                    Triangle trian2 = new Triangle(nonCommonPt1, nonCommonPt2, commonPts[1]);
                    triangles.RemoveAt(i);
                    triangles.Remove(this);
                    triangles.Add(trian1);
                    triangles.Add(trian2);
                    trian1.Legalize(triangles);
                    trian2.Legalize(triangles);
                    break;
                }
            }
            commonPts.Clear();
        }
    }
}
[Serializable()]
public class SaveFormat : ISerializable
{// the class to be serialized by wpf
    public List<List<Point>> l1 = new List<List<Point>>(); //ctrl points
    public List<Thickness> l2 = new List<Thickness>();     // marginns
    public List<List<Point>> l3 = new List<List<Point>>();  //Tangents
    public List<float> l4 = new List<float>();               //Altitudes
    public List<double> l5 = new List<double>();           //Stroke Thicknesses
    public string user;

    public SaveFormat()
    { }
    public SaveFormat(List<List<Point>> l1, List<Thickness> l2, List<List<Point>> l3, List<float> l4,List<double> l5,string user)
    {
        this.l1 = l1;
        this.l2 = l2;
        this.l3 = l3;
        this.l4 = l4;
        this.l5 = l5;
        this.user = user;
    }
    public SaveFormat(SerializationInfo info, StreamingContext context)
    {
        user = (string)info.GetValue("user",typeof( string));
        l1 = (List<List<Point>>)info.GetValue("list1", typeof(List<List<Point>>));
        l2 = (List<Thickness>)info.GetValue("list2", typeof(List<Thickness>));
        l3 = (List<List<Point>>)info.GetValue("list3", typeof(List<List<Point>>));
        l4 = (List<float>)info.GetValue("list4", typeof(List<float>));
        l5 = (List<double>)info.GetValue("list5", typeof(List<double>));
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("user", user);
        info.AddValue("list1", l1);
        info.AddValue("list2", l2);
        info.AddValue("list3", l3);
        info.AddValue("list4", l4);
        info.AddValue("list5", l5);
    }
}
 static class util
{

    public static void PrintCanvas( Canvas canvas, int dpi)
    {
        
        PrintDialog myPrintDialog = new PrintDialog();
        
        if (myPrintDialog.ShowDialog() == true)
        {
            myPrintDialog.PrintVisual(canvas, "Image Print");
        }
    }

    public static void SaveCanvas(Canvas canvas, int dpi, string filename)
    {
        Size size = new Size(canvas.Width, canvas.Height);
        canvas.Measure(size);
        var rtb = new RenderTargetBitmap(
            (int)canvas.Width, //width 
            (int)canvas.Height, //height 
            dpi, //dpi x 
            dpi, //dpi y 
            PixelFormats.Pbgra32 // pixelformat 
            );
        rtb.Render(canvas);

        SaveRTBAsPNG(rtb, filename);

    }

    private static void SaveRTBAsPNG(RenderTargetBitmap bmp, string filename)
    {
        var enc = new System.Windows.Media.Imaging.PngBitmapEncoder();
        enc.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));

        using (var stm = System.IO.File.Create(filename))
        {
            enc.Save(stm);
        }
    }

}