   M S B A M L   `   `   `  ����G  CComputerClub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:ComputerClub.ManagerComputerClub.Manager  5      +    %ComputerClub.Manager.EmployeesControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:ComputerClub.Manager     5      .��5
      � 
�������  � RowDefinitions  5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $1��+Управление работниками��6   $��24(�5      $
��Bold"�5      $��20,20,20,10q�5      ���#E31E245      �� 5   
   $��1��6   $
��Auto=�5      $
��Auto=�5      $��20,10,20,10q�5      .��5      &   System.Windows.Controls.DataGrid  -   5          Name$ dgEmployees��5         AutoGenerateColumns .     
IsReadOnly .    SelectionMode$ Single=�5      ���#E31E24   AlternatingRowBackground �	#FFE6E6E6
�������   GridLinesVisibility$	 All=�5#         HorizontalGridLinesBrush �	#FFC0C0C0   VerticalGridLinesBrush �	#FFC0C0C0	   	RowHeight$	 35Q�5&      
   ColumnHeaderStyle
 5)      �� 5*       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5+      ��  �� Property � �� �� Value$
 Bold"�67   �� 5,       � ��$ 14(�65   �� 5-       � �� �	#FFCE000067   �� 5.       � ��
 �����67   �� 5/       � ��$ 5q�64   �� 50       � ��$ Center=�6G   51         RowStyle 54      �� 55      )  #System.Windows.Controls.DataGridRow"  6   .�56      ��  � ��
 �   �67   �� 57       � ��$ Normal"�67   �� 58       � ��$ 5q�64    �� Triggers 59      P� 5:       P� Property �@J�IsMouseOver P� Value . . �5;   "   ��  � �� �	#FFE6E6E66?   5<      P� 5=       �@ 
IsSelected . . �5>   "   ��  � �� �	#FFCE00006?   �� 5?   "    � ��
 �����6?   5@      5B         Columns 5E      0  *System.Windows.Controls.DataGridTextColumn  5F      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$	 100 6L   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *Id+6L     5G      $ ФИО��6)   $	 260 6S    66   �� *
FullName+6S     5H      $ Телефон��6)   $	 150 6Z    6:   �� *PhoneNumber+6Z     5I      $ Должность��6)   $	 170 6Y    6<   �� *
Position+6Y     5J      $ Дата найма��6)   $	 120 6s    6=   �� *
HireDate+ �� StringFormat$ 
dd.MM.yyyy��6s   5L      5M      � 5O   
   $��2��6   $��20,10,20,20q�6    � ColumnDefinitions 5P      �� 5Q      $��* �6#   �� 5R      $��* �6#   .+�5U      �� -   5         ��Name$ btnAddEmployee��5U      $)��#Принять сотрудника��5V      $
��15,8q�5X      $��0,0,10,0q�5Y      ���#E31E24
�������$��0q�5\      $
��Bold"�6)   �� 5^      -   5        $ btnFireEmployee��5^      $��1��5_      $)��#Уволить сотрудника��5`      $
��15,8q�5b      $��10,0,0,0q�5c      ���#E31E24
�������$��0q�5f      $
��Bold"�6)   5g      5h      5i      