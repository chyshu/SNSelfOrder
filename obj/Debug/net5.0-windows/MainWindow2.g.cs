﻿#pragma checksum "..\..\..\MainWindow2.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "D94B3350B354238A9C6826CBD9DC1A8196C36839"
//------------------------------------------------------------------------------
// <auto-generated>
//     這段程式碼是由工具產生的。
//     執行階段版本:4.0.30319.42000
//
//     對這個檔案所做的變更可能會造成錯誤的行為，而且如果重新產生程式碼，
//     變更將會遺失。
// </auto-generated>
//------------------------------------------------------------------------------

using AxCSDEFTLib;
using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Core;
using Microsoft.Xaml.Behaviors.Input;
using Microsoft.Xaml.Behaviors.Layout;
using Microsoft.Xaml.Behaviors.Media;
using SNSelfOrder.Helpers;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
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


namespace SNSelfOrder {
    
    
    /// <summary>
    /// MainWindow2
    /// </summary>
    public partial class MainWindow2 : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 32 "..\..\..\MainWindow2.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Image test;
        
        #line default
        #line hidden
        
        
        #line 56 "..\..\..\MainWindow2.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid MainMenuGrid;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\MainWindow2.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox MainMenuListBox;
        
        #line default
        #line hidden
        
        
        #line 84 "..\..\..\MainWindow2.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid ItemGrid;
        
        #line default
        #line hidden
        
        
        #line 87 "..\..\..\MainWindow2.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListBox ItemsListBox;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.17.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/SNSelfOrder;component/mainwindow2.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\MainWindow2.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "5.0.17.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            
            #line 21 "..\..\..\MainWindow2.xaml"
            ((SNSelfOrder.MainWindow2)(target)).IsVisibleChanged += new System.Windows.DependencyPropertyChangedEventHandler(this.Window_IsVisibleChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.test = ((System.Windows.Controls.Image)(target));
            return;
            case 3:
            this.MainMenuGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 4:
            this.MainMenuListBox = ((System.Windows.Controls.ListBox)(target));
            
            #line 70 "..\..\..\MainWindow2.xaml"
            this.MainMenuListBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.MainMenuListBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 5:
            this.ItemGrid = ((System.Windows.Controls.Grid)(target));
            return;
            case 6:
            this.ItemsListBox = ((System.Windows.Controls.ListBox)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}
