﻿#pragma checksum "..\..\..\Pages\Window3D.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "126C8E294BDC5FA69AA52AFD6DBF04317A976DC5"
//------------------------------------------------------------------------------
// <auto-generated>
//     Ce code a été généré par un outil.
//     Version du runtime :4.0.30319.42000
//
//     Les modifications apportées à ce fichier peuvent provoquer un comportement incorrect et seront perdues si
//     le code est régénéré.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using TopoSurf.Pages;


namespace TopoSurf.Pages {
    
    
    /// <summary>
    /// Window3D
    /// </summary>
    public partial class Window3D : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 1 "..\..\..\Pages\Window3D.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal TopoSurf.Pages.Window3D DWindow;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\Pages\Window3D.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Header;
        
        #line default
        #line hidden
        
        
        #line 44 "..\..\..\Pages\Window3D.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Exit;
        
        #line default
        #line hidden
        
        
        #line 51 "..\..\..\Pages\Window3D.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid Delgrid;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\Pages\Window3D.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Slider SliderZoom;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/TopoSurf;component/pages/window3d.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Pages\Window3D.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.DWindow = ((TopoSurf.Pages.Window3D)(target));
            return;
            case 2:
            this.Header = ((System.Windows.Controls.Grid)(target));
            return;
            case 3:
            this.Exit = ((System.Windows.Controls.Button)(target));
            
            #line 44 "..\..\..\Pages\Window3D.xaml"
            this.Exit.Click += new System.Windows.RoutedEventHandler(this.Exit_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Delgrid = ((System.Windows.Controls.Grid)(target));
            
            #line 51 "..\..\..\Pages\Window3D.xaml"
            this.Delgrid.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.Delgrid_MouseDown);
            
            #line default
            #line hidden
            
            #line 51 "..\..\..\Pages\Window3D.xaml"
            this.Delgrid.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.Delgrid_MouseWheel);
            
            #line default
            #line hidden
            
            #line 51 "..\..\..\Pages\Window3D.xaml"
            this.Delgrid.MouseMove += new System.Windows.Input.MouseEventHandler(this.Delgrid_MouseMove);
            
            #line default
            #line hidden
            
            #line 51 "..\..\..\Pages\Window3D.xaml"
            this.Delgrid.MouseLeftButtonUp += new System.Windows.Input.MouseButtonEventHandler(this.Delgrid_MouseLeftButtonUp);
            
            #line default
            #line hidden
            return;
            case 5:
            this.SliderZoom = ((System.Windows.Controls.Slider)(target));
            
            #line 52 "..\..\..\Pages\Window3D.xaml"
            this.SliderZoom.ValueChanged += new System.Windows.RoutedPropertyChangedEventHandler<double>(this.SliderZoom_ValueChanged);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}
