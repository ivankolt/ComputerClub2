   M S B A M L   `   `   `  ����G  CComputerClub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null7 clr-namespace:ComputerClub.AdminComputerClub.Admin  5   	   *    $ComputerClub.Admin.EditProductWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   2local clr-namespace:ComputerClub.Admin     5   	     � Title$%  Изменение товара��5   	   $	��450��6"   $	��500��6/    � WindowStartupLocation$ CenterScreen=�5	   	   .��5
      � $��20q�6    � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $%��Изменение товара��6   $��24(�5      $
��Bold"�5      $��0,0,0,20q�5      ���	#FFCE00005      �� 5   
   $��1��6   $
��Auto=�6$   .��5      �� .+�5      �� $$��Название товара:��6   $��0,0,0,5q�5      $��SemiBold"�5      �� 5      -   5         ��Name$ txtProductName��5      $��0,0,0,15q�5      $��5q�5      �� 5       $��	Цена:��6   $��0,0,0,5q�5!      $��SemiBold"�5"      �� 5#      -   5        $ txtPrice��5#      $��0,0,0,15q�5$      $��5q�5%      �� 5'      $,��&Текущее изображение:��6   $��0,0,0,5q�5(      $��SemiBold"�5)      �� 5*      -   5         ��Name$ txtCurrentImage��5*      $��0,0,0,5q�5+      �� 5-      -   5         ��Name$ 
imgProduct��5-      $	��150��5.      $��Uniform=�5/      $��0,0,0,10q�50      $
��Left=�51      �� 53      -   5        $6��0Выбрать новое изображение��53      $
��10,5q�55      $��0,0,0,20q�56      ���	#FFCE0000
�������$
��Left=�59      5:      5;      �� 5=   
   $��2��6    �� Orientation$ 
Horizontal=�5>      $��Right=�5?      $��0,20,0,0q�5@      .+�5A      �� -   5        $��Отмена��5A      $
��15,8q�5C      $��0,0,10,0q�5D      
�������5E      �� 5F      -   5        $��Сохранить��5F      $
��15,8q�5H      ���	#FFCE0000
�������5J      5K      5L      5M      