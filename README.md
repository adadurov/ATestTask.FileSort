# ���������� ������� � ������� ��������� �����

������ ����������� �������� �������� ������ ��������, ����������� 2 ������� ��������� ������:

* ATestTask.FileSort.Sort.CLI.exe -- ���������� ������� � ��������� �����
* ATestTask.FileSort.Create.CLI.exe -- ���������� ��������� ������ ��������� ������� ��� ����������

## ����

* C# 9.0
* .NET 5.0

## ���������� ������� � ����� - ATestTask.FileSort.Sort.CLI.exe

### ���������

ATestTask.FileSort.Sort.CLI.exe sort <file_name> [<degree_of_parallelism> [<chunk_size_in_megabytes>]]

file_name -- ��� �������� �����, ���������� �������� ������ ���� �������������

mdegree_of_parallelism -- ������� �������������� ���������; 0 -- ������� �� ������ ����������� �� ����

chunk_size_in_megabytes -- ������������ ������ ������� ���������

### ��������

������� ��������� ������� ����, ��������� � ��������� ������ ������� External Sort � ��������� ��������� ���������� � ����� �����.
��� ��������� ����� -- <file_name>.sorted. ���� ������������� � ��� �� �����, � ������� ������������� ������� ����.
��� ���������� �������� ���������� ������������ ��������� �����, ����������� � �����, ���������� ������� ����. 
��� ����� ����������� �� ���������� ������ ��������.

������ ������ (�������) �������� ����� ������ ������� �� ������ ����������� �����, ����������� ". " � ������������� ������, ����������� ��������� ������� � �������.
��������� ���������� ������� � ������� ����������� ����.

���� �������� ���� ��� ����������, ������� �� ��������� ���������� � ��������� ������ � ��������� ����� ��������.

� �������� ������ ������� ������� � ������� �������������� ���������, � ��� ������� ������ -- ��������� �� �������.

#### ����������� ������

��� ������������ ���������, ������ ���������� ���������� �������� 2.5xS ���� RAM, ��� S -- ������ ���������.
��������, ��� ������������ ��������� 10 ���������� �������� 100 MiB, ����������� ������ �������� ����� 2.7 GiB.

#### �����������

1. ������� ���� ������ ���� � ASCII ���������.
2. ����������� ������� -- \n ��� \r\n
3. ������ �������� ������� ('\r'), ���� �� ������������ � ������������������, ����������� ������ �� ������� �����, ����������� ��� ����� ����������� ������.  
�� ���� ����, ��� ��� �������� �������������� �������� ����� 10, ��� ���������� �������� ������� ��������� ������������ � ����� �������� ���������, ����������� ��� ��������, ��� � ��������� ��������� �������.
4. ����� � ������ ������� �� ������ ��������� ������� ����� (leading zeroes).

### ������������

  1. ������� ������������ ����� ������������ ����� (��� ����������� � 32, 64 ���� � �.�.).

### ���� ����������� ���������

#### ���������� ������� ����������

1. ������� ����������� ��� ������ ���������� � �������� �� ����������� � �������� ���.  
������� ���������� ��������, ��� �� ���������� ��������� ����������� ������ (read ahead), ��� ������� �� ������ �����.
2. ���� ���� ��������, ��� ����� ������������� � ������ � ����� ���������, �� �������� �������������� "�����������" ������ ���������.
3. ��� ������������ ��������� ����� ����� ���������� ��������� � �������� �� ��������� ��������� ��������� �������� �������.
4. ��������� ���������� ������������ ����������� ����������.

#### �������� ����

* ������, ��� � ���� ���� �����, ���, ��� ����������, "������ ����� ������".  
  ����� ������� �����������, ����� ������ �� ����, �� ��� ���� ����������� �� ����������� ���������� ������������.
* ���� ����������� �������� ��������� ������������� ���� ����� ������������, ������������ ��������� �� ��������� � ����������� ����������.


## �������� �������� ������ - ATestTask.FileSort.Create.CLI.exe

### ���������

ATestTask.FileSort.Create.CLI.exe <file_name> <size_in_mebibytes>

### �������

ATestTask.FileSort.Create.CLI.exe g100.txt 102400

������� �������� ���� � �������� � ���� �� ����� 100 GiB �������� ������� (���������� ����� ����� ��������� �������� ������ �� ��������, �� ����������� 1 MiB.

### ��������� ����������

* CPU: Intel x86-64 (������ ����������� �� �������������) ��� x86 ���������� ���������� ������� �������������� ���������)
* RAM: ~10 MB per CPU core
* OS: Windows 10
* ��������� ����������: Microsoft .NET 5.0
