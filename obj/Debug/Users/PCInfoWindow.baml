   M S B A M L   `   `   `  ����  ComputerClub%    ComputerClub.Users.PCInfoWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   X TXceed.Wpf.Toolkit, Version=4.6.0.0, Culture=neutral, PublicKeyToken=3e4669d2f30244f44xctk)http://schemas.xceed.com/wpf/xaml/toolkit  5   	     � Title$"  Информация о ПК��5   	   $	��400��5   	   $	��500��5   	    � WindowStartupLocation$ CenterOwner=�5   	   
�������.��5
      � $��20q�6    � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��30 �6   �� 5      $��50 �6   .+�5   
   �� $��0��6   $'��!Характеристики ПК��6!   $
��Bold"�6:   $��20(�6L   $��0 0 0 10q�6Z   �� 5   
   $��1��6   $��Статус:��6!   $
��Bold"�60   �� 5   
   $��1��6   $��	100 0 0 0q�69   $
��Left=�6L   ��6!   �� *Status+6L   �� 5   
   $��2��6   $��Номер ПК:��6!   $
��Bold"�62   �� 5   
   $��2��6   $��	120 0 0 0q�67   $
��Left=�6J   ��6!   �� *PcId+6J   �� 5    
   $��3��6   $��Цена за час:��6!   $
��Bold"�65   �� 5!   
   $��3��6   $��	120 0 0 0q�6?   $
��Left=�6R   ��6!   �� *PricePerHour+6R   �� 5#   
   $��4��6   $��Видеокарта:��6!   $
��Bold"�64   �� 5$   
   $��4��6   $��	120 0 0 0q�6<   $
��Left=�6O   ��6!   �� *	VideoCard+6O   �� 5&   
   $��5��6   $��Процессор:��6!   $
��Bold"�63   �� 5'   
   $��5��6   $��	120 0 0 0q�66   $
��Left=�6I   ��6!   �� *CPU+6I   �� 5)   
   $��6��6   $,��&Начало бронирования:��6!   $
��Bold"�6=   &   Xceed.Wpf.Toolkit.DateTimePicker  5*   
   -   5          Name$ StartDateTimePicker��5,      $��6��5+      &   Xceed.Wpf.Toolkit.DateTimeUpDown   Format$ Custom=�5-         FormatString$ dd/MM/yyyy HH:mm��5.      $��	230,0,0,0q�5/      $
��Bold"�50      �  �Xceed.Wpf.Toolkit.Primitives.UpDownBase`1[[System.Nullable`1[[System.DateTime, mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]   Value 51      �� *	StartTime+ �� UpdateSourceTrigger$ PropertyChanged=��� 53   
   $��7��6   $2��,Окончание бронирования:��6!   $
��Bold"�6@     55   
   -   5        $ EndDateTimePicker��57      $��7��56      $ Custom=�58      $ dd/MM/yyyy HH:mm��59      $��	230,0,0,0q�5:      $
��Bold"�5;       5<      �� *	EndTime+$ PropertyChanged=��� 5?   
   $��8��6   $ ��Забронировать��5@      
����������	#FFCE0000$��100 20 100 0q�5C      $��16(�5D      $��Segoe UI)�5G      $
��Bold"�6'   ��5E      �� *BookCommand+{�5F      �� *IsAvailable+5G   '   5H      5I      