   M S B A M L   `   `   `  ����  ComputerClub    ComputerClub.Admin.Users   5      R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      .��5      � $��10q�6     � RowDefinitions  5	   
   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   $
��Auto=�5      $
��Auto=�5      $��	0,50,0,10q�5      .��5      &   System.Windows.Controls.DataGrid  -   5        $��dataGrid��5         AutoGenerateColumns .    
IsReadOnly .    CanUserAddRows .     CanUserDeleteRows .     CanUserResizeColumns .     CanUserReorderColumns .     CanUserSortColumns .     SelectionMode$ Single=�5      	   	RowHeight$	 35Q�5      $��14(�5      
   AlternatingRowBackground
 �	#FFE6E6E6
����������#E31E24$��1q�5!         GridLinesVisibility$	 All=�5"         HorizontalGridLinesBrush �	#FFC0C0C0   VerticalGridLinesBrush �	#FFC0C0C0   ColumnHeaderStyle 5'      �� 5(       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5)      ��  �� Property � �� �� Value$
 Bold"�67   �� 5*       � ��$ 14(�65   �� 5+       � �� �	#FFCE000067   �� 5,       � ��
 �����67   �� 5-       � ��$ 5q�64   �� 5.       � ��$ Center=�6G   5/         RowStyle 52      �� 53      )  #System.Windows.Controls.DataGridRow"  6   .�54      ��  � ��
 �   �67   �� 55       � ��$ Normal"�67   �� 56       � ��$ 5q�64   �� 57       �@�Cursor$
 Hand��63    �� Triggers 58      P� 59       P� Property �@J�IsMouseOver P� Value . . �5:   "   ��  � �� �	#FFE6E6E66?   5;      P� 5<       �@ 
IsSelected . . �5=   "   ��  � �� �	#FFCE00006?   �� 5>   "    � ��
 �����6?   5?      5A      5C      5D      �� 5F   
   $��0��6    �� Orientation$ 
Horizontal=�6"   $	��Top=�6;   $��0,3,0,10q�6S   .+�5G      �� $��Поиск:��6   $��Center=�6&   $��0 0 5 0q�6A   $
��Bold"�6R   �� 5H      -   5        $��	searchBox��5H      $	��200��6'   $��30��63   $��0 0 10 0q�6?   �� 5I      -   5        $!��filterActiveClientsCheckbox��5I      $8��2Показывать только активных��5J      $��0 0 0 10q�5K      $��10q�5L      $��16(�5M      $
��Bold"�5N      5P      �� 5R   
   $��1��6   $ 
Horizontal=�6"   $��Center=�6;   $
��0 10q�6X   .+�5S      �� -   5        $9��3Заблокировать пользователя��5S      ���	#FFCE0000
�������$	��200��5V      $��35��5W      $
��10 0q�5X      $
��Bold"�5Y      $��12(�5Z   *   �� 5\      -   5        $!��Пополнить счёт��5\      ���	#FFCE0000
�������$	��160��5_      $��35��5`      $
��Bold"�5a      $
��10 0q�5b      $��12(�5c   ,   �� 5e      -   5        $@��:Сформировать отчёт активностей��5e      ���	#FFCE0000
�������$	��220��5h      $��35��5i      $
��Bold"�5j      $
��10 0q�5k      $��12(�5l      6   5m      5n      5o      