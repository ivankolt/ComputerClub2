   M S B A M L   `   `   `  ����G  CComputerClub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null;"clr-namespace:ComputerClub.ManagerComputerClub.Manager  5      '    !ComputerClub.Manager.UsersControl   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      4local"clr-namespace:ComputerClub.Manager     5      .��5	      � 
�������  � RowDefinitions  5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $7��1Управление пользователями��6   $��24(�5      $
��Bold"�5      $��20,20,20,10q�5      ���#E31E245      �� 5   
   $��1��6   $
��Auto=�5      $
��Auto=�5      $��20,10,20,10q�5      .��5      &   System.Windows.Controls.DataGrid  -   5          Name$ dgUsers��5         AutoGenerateColumns .     
IsReadOnly .    SelectionMode$ Single=�5      ���#E31E24   AlternatingRowBackground �	#FFE6E6E6
�������   GridLinesVisibility$	 All=�5!         HorizontalGridLinesBrush �	#FFC0C0C0   VerticalGridLinesBrush �	#FFC0C0C0	   	RowHeight$	 35Q�5$      
   ColumnHeaderStyle
 5'      �� 5(       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5)      ��  �� Property � �� �� Value$
 Bold"�67   �� 5*       � ��$ 14(�65   �� 5+       � �� �	#FFCE000067   �� 5,       � ��
 �����67   �� 5-       � ��$ 5q�64   �� 5.       � ��$ Center=�6G   5/         RowStyle 52      �� 53      )  #System.Windows.Controls.DataGridRow"  6   .�54      ��  � ��
 �   �67   �� 55       � ��$ Normal"�67   �� 56       � ��$ 5q�64    �� Triggers 57      P� 58       P� Property �@J�IsMouseOver P� Value . . �59   "   ��  � �� �	#FFE6E6E66?   5:      P� 5;       �@ 
IsSelected . . �5<   "   ��  � �� �	#FFCE00006?   �� 5=   "    � ��
 �����6?   5>      5@         Columns 5C      0  *System.Windows.Controls.DataGridTextColumn  5D      ,  &System.Windows.Controls.DataGridColumn   Header$ ID��6)      Width5  /System.Windows.Controls.DataGridLengthConverter$ 50 6L   1  +System.Windows.Controls.DataGridBoundColumn   Binding 65   �� *Id+6L     5E      $% Имя пользователя��6)   $	 150 6`    6C   �� *
Username+6`     5F      $ ФИО��6)   $	 150 6S    66   �� *
FullName+6S     5G      $ Email��6)   $	 180 6R    68   �� *Email+6R     5H      $ Баланс��6)   $	 100 6U    69   �� *	Balance+6U   5J      5K      �� 5M   
   -   5         ��Name$ btnUnblockUser��5M      $��2��5N      $;��5Разблокировать пользователя��5O      $
��15,8q�5Q      $��20,10,20,20q�5R      ���	#FFCE0000
�������$��0q�5U      $��Center=�5V      $
��Bold"�6.   �� 5W   
   -   5        $@��:Сформировать отчёт активностей��5W      $��2��6;   ���	#FFCE0000
�������$	��220��5Z      $��35��5[      $
��Bold"�5\      $��500,0,20,10q�5]      $��12(�5^      6   5_      5`      