﻿#pragma checksum "..\..\..\Manager\AddPCControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "80CD1170300AAC56434B999801B9F4735C4D94366C23310EF2CB138D252392A9"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using ComputerClub.Manager;
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


namespace ComputerClub.Manager {
    
    
    /// <summary>
    /// AddPCControl
    /// </summary>
    public partial class AddPCControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 44 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtPricePerHour;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbZone;
        
        #line default
        #line hidden
        
        
        #line 60 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbEquipment;
        
        #line default
        #line hidden
        
        
        #line 65 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnAddPC;
        
        #line default
        #line hidden
        
        
        #line 99 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.DataGrid dgPCs;
        
        #line default
        #line hidden
        
        
        #line 153 "..\..\..\Manager\AddPCControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button btnDeletePC;
        
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
            System.Uri resourceLocater = new System.Uri("/ComputerClub;component/manager/addpccontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Manager\AddPCControl.xaml"
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
            this.txtPricePerHour = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            this.cmbZone = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 3:
            this.cmbEquipment = ((System.Windows.Controls.ComboBox)(target));
            return;
            case 4:
            this.btnAddPC = ((System.Windows.Controls.Button)(target));
            
            #line 67 "..\..\..\Manager\AddPCControl.xaml"
            this.btnAddPC.Click += new System.Windows.RoutedEventHandler(this.btnAddPC_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.dgPCs = ((System.Windows.Controls.DataGrid)(target));
            return;
            case 6:
            this.btnDeletePC = ((System.Windows.Controls.Button)(target));
            
            #line 156 "..\..\..\Manager\AddPCControl.xaml"
            this.btnDeletePC.Click += new System.Windows.RoutedEventHandler(this.btnDeletePC_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

