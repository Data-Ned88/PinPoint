﻿#pragma checksum "..\..\..\modals\ConfirmPublish.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "DEAC8C15226C0F7C52D26E30DCE930016B41CB1369ED754F7AB9B23E9C386936"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using PinpointUI.modals;
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


namespace PinpointUI.modals {
    
    
    /// <summary>
    /// ConfirmPublish
    /// </summary>
    public partial class ConfirmPublish : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid OptionsGrid;
        
        #line default
        #line hidden
        
        
        #line 31 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox FontListBox;
        
        #line default
        #line hidden
        
        
        #line 37 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox FontSizeListBox;
        
        #line default
        #line hidden
        
        
        #line 45 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid colourThemeGrid;
        
        #line default
        #line hidden
        
        
        #line 74 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ButtonGrid;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnConfirm;
        
        #line default
        #line hidden
        
        
        #line 83 "..\..\..\modals\ConfirmPublish.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnCancel;
        
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
            System.Uri resourceLocater = new System.Uri("/PinpointUI;component/modals/confirmpublish.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\modals\ConfirmPublish.xaml"
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
            this.OptionsGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 2:
            this.FontListBox = ((System.Windows.Controls.ListBox)(target));
            
            #line 31 "..\..\..\modals\ConfirmPublish.xaml"
            this.FontListBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.FontListBox_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 32 "..\..\..\modals\ConfirmPublish.xaml"
            this.FontListBox.Loaded += new System.Windows.RoutedEventHandler(this.FontListBox_Loaded);
            
            #line default
            #line hidden
            return;
            case 3:
            this.FontSizeListBox = ((System.Windows.Controls.ListBox)(target));
            
            #line 37 "..\..\..\modals\ConfirmPublish.xaml"
            this.FontSizeListBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.FontSizeListBox_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 38 "..\..\..\modals\ConfirmPublish.xaml"
            this.FontSizeListBox.Loaded += new System.Windows.RoutedEventHandler(this.FontSizeListBox_Loaded);
            
            #line default
            #line hidden
            return;
            case 4:
            this.colourThemeGrid = ((System.Windows.Controls.DataGrid)(target));
            
            #line 51 "..\..\..\modals\ConfirmPublish.xaml"
            this.colourThemeGrid.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.colourThemeGrid_SelectionChanged);
            
            #line default
            #line hidden
            
            #line 52 "..\..\..\modals\ConfirmPublish.xaml"
            this.colourThemeGrid.Loaded += new System.Windows.RoutedEventHandler(this.colourThemeGrid_Loaded);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ButtonGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.btnConfirm = ((System.Windows.Controls.Button)(target));
            
            #line 82 "..\..\..\modals\ConfirmPublish.xaml"
            this.btnConfirm.Click += new System.Windows.RoutedEventHandler(this.btnConfirm_Click);
            
            #line default
            #line hidden
            return;
            case 7:
            this.btnCancel = ((System.Windows.Controls.Button)(target));
            
            #line 83 "..\..\..\modals\ConfirmPublish.xaml"
            this.btnCancel.Click += new System.Windows.RoutedEventHandler(this.btnCancel_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

