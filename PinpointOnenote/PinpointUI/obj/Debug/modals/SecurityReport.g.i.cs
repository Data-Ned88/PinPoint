﻿#pragma checksum "..\..\..\modals\SecurityReport.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "1C71889AA3E14032D0F998E3E8FA8E2E16E799AC6222F2ACBFBEEC81D41B1FB4"
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
    /// SecurityReport
    /// </summary>
    public partial class SecurityReport : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 90 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockPassBankName;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock textBlockPassBankDVS;
        
        #line default
        #line hidden
        
        
        #line 112 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeViewItem treeViewSingleLogins;
        
        #line default
        #line hidden
        
        
        #line 114 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeViewItem treeViewExactShares;
        
        #line default
        #line hidden
        
        
        #line 116 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TreeViewItem treeViewStems;
        
        #line default
        #line hidden
        
        
        #line 128 "..\..\..\modals\SecurityReport.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnExit;
        
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
            System.Uri resourceLocater = new System.Uri("/PinpointUI;component/modals/securityreport.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\modals\SecurityReport.xaml"
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
            this.textBlockPassBankName = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 2:
            this.textBlockPassBankDVS = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 3:
            this.treeViewSingleLogins = ((System.Windows.Controls.TreeViewItem)(target));
            return;
            case 4:
            this.treeViewExactShares = ((System.Windows.Controls.TreeViewItem)(target));
            return;
            case 5:
            this.treeViewStems = ((System.Windows.Controls.TreeViewItem)(target));
            return;
            case 6:
            this.btnExit = ((System.Windows.Controls.Button)(target));
            
            #line 128 "..\..\..\modals\SecurityReport.xaml"
            this.btnExit.Click += new System.Windows.RoutedEventHandler(this.btnExit_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

