   M S B A M L   `   `   `  ����G  CComputerClub, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullE'clr-namespace:ComputerClub.RegistrationComputerClub.Registration  5      '    !ComputerClub.Registration.Registr   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      9local'clr-namespace:ComputerClub.Registration     5        � Title$  Registr��5	      $	��645��6   $	��875��6$    � WindowStartupLocation$ CenterScreen=�60    � WindowStyle$
 None=�6U    � AllowsTransparency . 
������ $��18(�6�   .��5
      � $��20q�6    � ColumnDefinitions 5      �� 5   
   $��* �6   �� 5   
   $��1.25* �6    J� Effect 5      3  -System.Windows.Media.Effects.DropShadowEffect  5   
      
BlurRadius$ 30Q�6      ShadowDepth$ 1Q�6+      Opacity$	 0.4Q�6;   .+�5      �� 
���   �	 �� CornerRadius$	 	10 0 0 10��6    }�. 
 J� Clip
 5   
   �� 5       �� Rect$ 0,0,400,605�6     �� RadiusX$ 10Q�63    �� RadiusY$ 10Q�6@   ._�5   
   � .+�5      �� $��	0 120 0 0q�6   $	��Top=�6,   .+�5      �� $��Light"�6   $��18(�6/   
������� �� TextAlignment$ Center=�6P   �� 6h   $�Введите код��6l   $
��Bold"�6   $��36(�6�   �� 6�   �� 6�   6�   �� 5      $��Light"�6   $��18(�6/   
�������$ Center=�6P   5      A� 5      $	��500��6   $	��500��6"   $��Bottom=�6/   ���	#FFCE0000$��50 0 0 -200q�5   +    J� Opacity$	 0.6Q�6@   A� 5       $	��300��6   $	��300��6"   $��Bottom=�6/   ���	#FFCE0000$��-450 0 0 30q�5!   +   $	 0.3Q�6@   A� 5"      $	��400��6   $	��400��6"   $��Bottom=�6/   ���	#FFCE0000$��-320 0 0 -120q�5#   +   $	 0.3Q�6B   A� 5$      $	��250��6   $	��250��6"   $��Bottom=�6/   ���	#FFCE0000$��-380 0 0 -120q�5%   +   A� 5&      $	��400��6   $	��400��6"   $��Bottom=�6/   ���	#FFCE0000$��280 0 0 -70q�5'   +   $	 0.8Q�6@   5)      5*      �� 5+      $��1��6   
�������$	 	0 10 10 0��60   ._�5,   
   � $��	60 0 60 0q�6    � RowDefinitions 5-      �� 5.      $��45 �6    �� 5/      $��2* �6    �� 50      $��* �6    .+�53      �� $��1��6   $��	0 100 0 0q�6&   .+�54      �� -   5        ���	#FFCE0000$	��320��54   0   $	 5��6<   $��40��6M   $��6q�6Y   $	 0.7Q�6d   ._�55      �� -   5         ��Name$ PasswordMail��55      
������ $��0q�6M   $	��300��6a   $��Код��6m   $��Center=�6x   58      5;      �� 5<      $	��Top=�6   $��2��6-   ���	#FFCE0000$��50��6Q   $	��250��6]   $	 10��6i   ._�5=      �� -   5        $#��Подтвердить код��5=      
������ $��5q�6L   
�������$��23(�6j   $��0q�6x   $
��Bold"�6�   5?      5A      5C      5E      5F      5G      