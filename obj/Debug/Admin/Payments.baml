   M S B A M L   `   `   `  ����  ComputerClub!    ComputerClub.Admin.Payments   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$%  История платежей��5   	    � WindowStartupLocation$ CenterScreen=�5   	   $	��450��5   	   $	��950��6   .��5      � $��10q�6    � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��Right=�6.   $��0��6J   $��0 0 0 10q�6W   .+�5      �� -   5         ��Name$ GenerateReportButton��5      $)��#Сформировать отчёт��5      $	��150��5      $��30��5      $��14(�5      ���	#FFCE0000
����������	#FFA80000$
��Bold"�5   4   5      &   System.Windows.Controls.DataGrid  5   
   -   5          Name$ PaymentsHistoryGrid��5      $��1��5         AutoGenerateColumns .     CanUserAddRows .     
IsReadOnly . $��0 20 0 0q�5      	   	RowHeight$	 35Q�5      $��14(�5      
   AlternatingRowBackground
 �	#FFE6E6E6
����������	#FFC0C0C0$��1q�5#         GridLinesVisibility$	 All=�5$         HorizontalGridLinesBrush �	#FFC0C0C0   VerticalGridLinesBrush �	#FFC0C0C0   ColumnHeaderStyle 5)      �� 5*       �� 
TargetType=  7System.Windows.Controls.Primitives.DataGridColumnHeader"  6   .�5+      ��  �� Property � �� �� Value$
 Bold"�63   �� 5,       � ��$ 14(�61   �� 5-       � �� �	#FFCE000063   �� 5/       � ��
 �����63   �� 50       � ��$ 5q�60   �� 51       � ��$ Center=�6C   52         RowStyle 55      �� 56      )  #System.Windows.Controls.DataGridRow"  6   .�57      ��  � ��
 �   �63   �� 58       � ��$ Normal"�63   �� 59       � ��$ 5q�60    �� Triggers 5;      P� 5<       P� Property �@J�IsMouseOver P� Value . . �5=      ��  � �� �	#FFE6E6E66;   5>      P� 5?       �@ 
IsSelected . . �5@      ��  � �� �	#FFCE00006;   �� 5A       � ��
 �����6;   5B      5D         Columns 5G      0  *System.Windows.Controls.DataGridTextColumn  5H      ,  &System.Windows.Controls.DataGridColumn   Header$ ID Платежа��5I         Width5  /System.Windows.Controls.DataGridLengthConverter$
 Auto 5K      1  +System.Windows.Controls.DataGridBoundColumn   Binding 5J      �� *Id+5K        5L      $ Пользователь��5M      $
 1.5* 5O       5N      �� *
Username+5O        5P      $ 
Сумма��5Q      $ * 5S       5R      �� *Amount+5S        5T      $ Тип��5U      $ * 5W       5V      �� *TypePayment+5W        5X      $ Дата платежа��5Y      $
 1.8* 5[       5Z      �� *DatePayment+ �� StringFormat$ {0:dd.MM.yyyy HH:mm}��5[        5\      $ Услуга/Товар��5]      $
 1.5* 5_       5^      �� *ServiceName+5_        5`      $ Номер счета��5a      $ 2* 5c       5b      �� *AccountNumber+5c        5d      $! ID Пользователя��5e      $
 Auto 5g       5f      �� *UserId+5g      5i      5j      5k      