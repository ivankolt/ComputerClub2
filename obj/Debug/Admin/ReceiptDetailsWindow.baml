   M S B A M L   `   `   `  ����  ComputerClub-    'ComputerClub.Admin.ReceiptDetailsWindow   5   	   R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Детали чека��5   	    � WindowStartupLocation$ CenterScreen=�5   	   $	��550��5   	   $	��450��6   .��5      � $��20q�6    � RowDefinitions 5   
   �� 5	      $
��Auto �6   �� 5
      $��* �6   �� 5      $
��Auto �6   .+�5   
   �� $��0��6   
����������	#FFCE0000$��1q�6I    �� Padding$ 15q�6]   $��0,0,0,15q�6j   ._�5      �� .+�5      �� $'��!Компьютерный клуб��6   $��20(�65   $
��Bold"�6C   $��Center=�6U   $��0,0,0,5q�6r   �� 5      $��ИНН: 123456789012��6   $��12(�65   $��Center=�6C   $��0,0,0,5q�6`   �� 5      $<��6г. Магнитогорск, ул. Труда, д. 26��6   $��12(�6E   $��Center=�6S   �� 5      $��	0,10,0,10q�6   � 5       � ColumnDefinitions 5      �� 5      $
��Auto �6+   �� 5      $��* �6+    5      �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   �� 5      $
��Auto �6(   .+�5!      �� $��0��6    $��0��6-   $��Чек №:��6=   $
��Bold"�6K   $��0,0,10,5q�6]   �� 5"      $��0��6    $��1��6-   $��0,0,0,5q�6X   ��6=   �� *	ReceiptId+6X   �� 5$      $��1��6    $��0��6-   $��	Дата:��6=   $
��Bold"�6J   $��0,0,10,5q�6\   �� 5%      $��1��6    $��1��6-   $��0,0,0,5q�6�   ��6=   �� *	CreatedAt+ �� StringFormat$ {0:dd.MM.yyyy HH:mm:ss}��6�   �� 5'      $��2��6    $��0��6-   $��Кассир:��6=   $
��Bold"�6L   $��0,0,10,5q�6^   �� 5(      $��2��6    $��1��6-   $��0,0,0,5q�6Z   ��6=   �� *CashierName+6Z   �� 5*      $��3��6    $��0��6-   $��Клиент:��6=   $
��Bold"�6L   $��0,0,10,5q�6^   �� 5+      $��3��6    $��1��6-   $��0,0,0,5q�6[   ��6=   �� *CustomerName+6[   5,      5-      5.      �� 5/   
   $��1��6   
�������$	��230��61   ���	#FFCE0000$��1q�6V   $ 15q�6j   ._�50      �  51      �� 52      $
��Auto �6$   �� 53      $��* �6$   �� 54      $
��Auto �6$   .+�57      �� $��Позиции чека��6   $
��Bold"�60   $��0,0,0,10q�6B   �� 5:      $��1��6   $
��Auto=�6,   .��5;      �� $��0q�6=   $��Disabled=�5<      ��5;      �� *Items+ �� View 5=      �� 5>      .P�5?   "   �� $��Наименование��61    �� Width$	 180��6G    �� DisplayMemberBinding 6S   �� *Name+�� 5@   "   $��Кол-во��61   $ 60��6A    6L   �� *
Quantity+�� 5A   "   $��Цена��61   $ 70��6?    6J   �� *Price+�� 5B   "   $��
Сумма��61   $ 70��6@    6K   �� *Total+5C      5E      5F      �� 5H      $��2��6   $��	0,15,0,15q�6)   � 5J      $��3��6    5K      �� 5L      $��* �6+   �� 5M      $
��Auto �6+    5O      �� 5P      $
��Auto �6(   �� 5Q      $
��Auto �6(   .+�5T      �� $��0��6    $��0��6-   $��Итого:��6=   $
��Bold"�6K   $��Right=�6]   $��0,0,10,5q�6y   �� 5U      $��0��6    $��1��6-   $
��Bold"�6Z   $��0,0,0,5q�6l   ��6=   �� *TotalAmount+6l   �� 5W      $��1��6    $��0��6-   $ ��Способ оплаты:��6=   $
��Bold"�6S   $��Right=�6e   $��0,0,10,5q�6�   �� 5X      $��1��6    $��1��6-   $��0,0,0,5q�6\   ��6=   �� *PaymentMethod+6\   5Y      5Z      5[      � 5]   
   $��2��6   $��0,15,0,0q�6    5^      �� 5_      $��* �6#   �� 5`      $��* �6#   .+�5c      �� -   5        $��0��5c      $��Скачать чек��6%   $��Center=�5d      $
��20,5q�5e      ���	#FFCE0000
�������$
��Bold"�5h      $��0,0,5,0q�5i      5j      �� 5l      -   5        $��1��5l      $��Закрыть��6%   $��Center=�5m      $
��20,5q�5n      ���	#FFCE0000
�������$
��Bold"�5q      $��5,0,0,0q�5r      5s      5t      5u      5v      