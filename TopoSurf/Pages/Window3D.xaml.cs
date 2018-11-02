using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace TopoSurf.Pages
{
    /// <summary>
    /// Interaction logic for Window3D.xaml
    /// </summary>
    public partial class Window3D : Window
    {
        public Window3D()
        {
            InitializeComponent();
            SliderZoom.Value = -Math.Sqrt(44); // initialisation du valeur du slider

        }

        Point pointPreced; // les coordonnes des point precedent du cursor
        Boolean CHangeCamera = false; // si l'utilasateur est selectionner la forme alors changeCamera est devient vraie
        Viewport3D view;
        double P = Math.Sqrt(44), P1 = Math.Sqrt(44);
        private void Delgrid_MouseDown(object sender, MouseButtonEventArgs e)
        {


            object TestPanelOrUI = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (TestPanelOrUI != null)
            {
                if (!(TestPanelOrUI is Grid))
                {
                    if (TestPanelOrUI is Viewport3D) // si l'utilisateur a selectionner la forme
                    {

                        CHangeCamera = true;
                        pointPreced = e.GetPosition(this); // les nouveau coordinnes des pointpreced
                        Viewport3D viewport = (Viewport3D)TestPanelOrUI;
                        view = viewport;
                    }
                }

            }

        }

        private void Delgrid_MouseMove(object sender, MouseEventArgs e)
        {
            object TestPanelOrUI = this.InputHitTest(e.GetPosition(this)) as FrameworkElement;
            if (TestPanelOrUI != null)
            {
                if (CHangeCamera)
                {

                    Viewport3D viewport = view;
                    /** Pour changer la direction qu'a  partir duquel  nous voyons la fomre  on utilise les coordonnes spherique pour changer l'empalecment du camera */
                    double p = P1; //le rayon du sphere
                    double r = Math.Sqrt(((PerspectiveCamera)view.Camera).Position.X * ((PerspectiveCamera)view.Camera).Position.X + ((PerspectiveCamera)view.Camera).Position.Z * ((PerspectiveCamera)view.Camera).Position.Z);
                    double cosO;
                    if (r != 0)
                        cosO = ((PerspectiveCamera)view.Camera).Position.Z / r;
                    else
                        cosO = 0;
                    double sinO;
                    if (r != 0)
                        sinO = ((PerspectiveCamera)view.Camera).Position.X / r;
                    else
                        sinO = 0;
                    double cosB = ((PerspectiveCamera)view.Camera).Position.Y / p;
                    double sinB = r / p;
                    double O = Math.Acos(cosO);     //recherchons la valeur de l'angle O             
                    if (sinO < 0)
                    {
                        O = O * (-1);
                    }
                    if (e.GetPosition(this).X < pointPreced.X) // on change Camera vers la droit 
                    {

                        O += 0.05; // augmentation du l'angle O
                        sinO = Math.Sin(O);
                        cosO = Math.Cos(O);

                    }
                    else if (e.GetPosition(this).X > pointPreced.X) // on change Camera vers la gauche
                    {

                        O -= 0.05; // reduction de l'angle O
                        sinO = Math.Sin(O);
                        cosO = Math.Cos(O);

                    }
                    double B = Math.Acos(cosB); //recherchons la valeur de l'angle B
                    if (sinB < 0)
                    {
                        B = B * (-1);
                    }
                    if (e.GetPosition(this).Y > pointPreced.Y) // on change Camera vers le haut 
                    {

                        B -= 0.05;
                        if (B < 0)
                        {
                            B = 0.04;
                        }
                        sinB = Math.Sin(B);
                        cosB = Math.Cos(B);

                    }
                    else if (e.GetPosition(this).Y < pointPreced.Y) // on change Camera vers le bas 
                    {

                        B += 0.05;
                        if (B > Math.PI)
                        {
                            B = Math.PI;
                        }

                        sinB = Math.Sin(B);
                        cosB = Math.Cos(B);

                    }
                    p = P;
                    P1 = P;
                    ((PerspectiveCamera)viewport.Camera).Position = new Point3D(p * sinB * sinO, p * cosB, p * sinB * cosO); // changement de la valeur du camera
                    ((PerspectiveCamera)viewport.Camera).LookDirection = new Vector3D(-((PerspectiveCamera)viewport.Camera).Position.X, -((PerspectiveCamera)viewport.Camera).Position.Y, -(((PerspectiveCamera)viewport.Camera).Position.Z));
                    pointPreced = e.GetPosition(this); // les nouveau coordonees pointPreced
                }


            }
        }
        private void Delgrid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CHangeCamera)
            {
                CHangeCamera = false;
                //view = null;
            }
        }


        private void SliderZoom_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Viewport3D viewport = view;
            if (view == null)
            {

                foreach (UIElement element in Delgrid.Children)
                {
                    if (element is Viewport3D)
                    {
                        viewport = (Viewport3D)element;
                        break;
                    }
                }
                view = viewport;
            }
            if (viewport != null)
            {

                P = (-1) * SliderZoom.Value;
                double p = P1;
                double r = Math.Sqrt(((PerspectiveCamera)viewport.Camera).Position.X * ((PerspectiveCamera)viewport.Camera).Position.X + ((PerspectiveCamera)viewport.Camera).Position.Z * ((PerspectiveCamera)viewport.Camera).Position.Z);
                double cosO;
                if (r != 0)
                    cosO = ((PerspectiveCamera)viewport.Camera).Position.Z / r;
                else
                    cosO = 0;
                double sinO;
                if (r != 0)
                    sinO = ((PerspectiveCamera)viewport.Camera).Position.X / r;
                else
                    sinO = 0;
                double cosB = ((PerspectiveCamera)viewport.Camera).Position.Y / p;
                double sinB = r / p;
                p = P;
                P1 = P;
                ((PerspectiveCamera)viewport.Camera).Position = new Point3D(p * sinB * sinO, p * cosB, p * sinB * cosO);
                ((PerspectiveCamera)viewport.Camera).LookDirection = new Vector3D(-((PerspectiveCamera)viewport.Camera).Position.X, -((PerspectiveCamera)viewport.Camera).Position.Y, -(((PerspectiveCamera)viewport.Camera).Position.Z));


            }

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
            
        }

        private void Delgrid_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Viewport3D viewport = view;
            if (view == null)
            {

                foreach (UIElement element in Delgrid.Children)
                {
                    if (element is Viewport3D)
                    {
                        viewport = (Viewport3D)element;
                        break;
                    }
                }
                view = viewport;
            }


            if ((P - e.Delta * 0.01) >= 3.5 && (P - e.Delta * 0.01) <= 20)
            {
                P = P - e.Delta * 0.01;
                double p = P1;
                double r = Math.Sqrt(((PerspectiveCamera)viewport.Camera).Position.X * ((PerspectiveCamera)viewport.Camera).Position.X + ((PerspectiveCamera)viewport.Camera).Position.Z * ((PerspectiveCamera)viewport.Camera).Position.Z);
                double cosO;
                if (r != 0)
                    cosO = ((PerspectiveCamera)viewport.Camera).Position.Z / r;
                else
                    cosO = 0;
                double sinO;
                if (r != 0)
                    sinO = ((PerspectiveCamera)viewport.Camera).Position.X / r;
                else
                    sinO = 0;
                double cosB = ((PerspectiveCamera)viewport.Camera).Position.Y / p;
                double sinB = r / p;
                p = P;
                P1 = P;
                ((PerspectiveCamera)viewport.Camera).Position = new Point3D(p * sinB * sinO, p * cosB, p * sinB * cosO);
                ((PerspectiveCamera)viewport.Camera).LookDirection = new Vector3D(-((PerspectiveCamera)viewport.Camera).Position.X, -((PerspectiveCamera)viewport.Camera).Position.Y, -(((PerspectiveCamera)viewport.Camera).Position.Z));
                SliderZoom.Value = -P;
            }

        }
    }
}