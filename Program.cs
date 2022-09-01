using System;
using System.Collections.Generic;
using System.IO;

namespace gost {
    class Program {
        string[] key = new string[8];
        string[] K = new string[8];
        string resKey = "";
        // регламентированная таблица замены
        char[,] sbox = {
                {'4','A','9','2','D','8','0','E','6','B','1','C','7','F','5','3'},
                {'E','B','4','C','6','D','F','A','2','3','8','1','0','7','5','9'},
                {'5','8','1','D','A','3','4','2','E','F','C','7','6','0','9','B'},
                {'7','D','A','1','0','8','9','F','E','4','6','C','B','2','5','3'},
                {'6','C','7','1','5','F','D','8','4','A','9','E','0','3','B','2'},
                {'4','B','A','0','7','2','1','D','3','6','8','5','9','C','F','E'},
                {'D','B','4','1','3','F','5','9','0','A','E','7','6','8','2','C'},
                {'1','F','D','0','5','7','A','4','9','2','3','E','6','B','8','C'} 
        };
        string text;

        //функция генерации ключа
        void KeyGen() {
            
            // номер подключа
            int z = 0;
            // переменная для записи подключа
            string key1 = "";
            // собранный ключ в бинарной системе
            string binkey = "";
            Random rnd = new Random();
            
            // циклом собирается брандомнй бинарный ключ в переменную key1
            for (int i = 0; i < 32; i++) {
                string k = "";
                for (int j = 0; j < 8; j++) {
                    k += Convert.ToString(rnd.Next(0, 2));
                }
                binkey += k;
                k = Convert.ToInt32(k, 2).ToString("X");
                if (k.Length < 2)
                    k = "0" + k;
                key1 += k;
                if ((i + 1) % 4 == 0) {
                    key[z] = key1;
                    key1 = "";
                    resKey += key[z] + " ";
                    z++;
                }
            }
            z = 0;
            // функция, которая записывает в К (глоб.переменная) ключ по 8 частей (32 бита)
            for (int i = 0; i < 256; i++) {
                K[z] += binkey[i];
                if ((i + 1) % 32 == 0)
                    z++;
                
            }
        }
        
        // Перевод в 16-ти ричную систему ключа из файла
        void GetKey() {
            StreamReader sr = new StreamReader("output.txt");
            sr.ReadLine();
            string ki = sr.ReadLine();
            ki = ki.Remove(ki.Length - 1);
            key = ki.Split(" ");
            for (int i = 0; i < key.Length; i++) {
                Console.WriteLine("key: " + key[i]);
                resKey += key[i] + " ";
            }
            sr.Close();
            Dictionary<string, string> Hex = new Dictionary<string, string>() {
                { "0", "0000"},
                { "1", "0001"},
                { "2", "0010"},
                { "3", "0011"},
                { "4", "0100"},
                { "5", "0101"},
                { "6", "0110"},
                { "7", "0111"},
                { "8", "1000"},
                { "9", "1001"},
                { "A", "1010"},
                { "B", "1011"},
                { "C", "1100"},
                { "D", "1101"},
                { "E", "1110"},
                { "F", "1111"}
            };

            
            for (int i = 0; i < key.Length; i++) {
                K[i] = "";
                for (int j = 0; j < key[i].Length; j++) {
                    
                    K[i] += Hex[key[i][j].ToString()];
                }
            }
        }

        //алгоритм шифрования
        string Crypt() {
            string Result = "";

            //создаем поток записи
            StreamReader sr = new StreamReader("input.txt");
            string text = sr.ReadLine();
            sr.Close();
            // Если тексту не достает длинны - добавляется пробелы
            while (text.Length % 8 != 0 || text.Length < 8){
                text += " ";
            }

            // 
            for (int i = 0; i < text.Length; i += 8) {
                // Считывается текст из файла и забираем по 8 символов (делим на 2 по 4 символа  А и В)
                string A = "" + text[i] + text[i + 1] + text[i + 2] + text[i + 3];
                string B = "" + text[i + 4] + text[i + 5] + text[i + 6] + text[i + 7];
                // Переводим в бинарный вид
                string binA = Bin(A);
                string binB = Bin(B);
                
                // Кодирование всего длока
                for (int j = 0; j < 24; j++) {
                    string f = F(binA, K[j % 8]);
                    string xor = Xor(binB, f);
                    binB = binA;
                    binA = xor;
                }
                
                // Цикл для кодирование с конца данных  8 блоков
                for (int j = 7; j >= 0; j--) {
                    string f = F(binA, K[j]);
                    string xor = Xor(binB, f);
                    binB = binA;
                    binA = xor;
                }
                Result += BinToDec(binB) + BinToDec(binA);
            }
            return (Result);
        }

        //алгоритм расшифровывания
        string Decrypt() {
            string Result = "";

            //создаем поток записи
            StreamReader sr = new StreamReader("output.txt");
            string text = sr.ReadLine();
            sr.Close();
            for (int i = 0; i < text.Length; i += 8) {
                string A = "" + text[i] + text[i + 1] + text[i + 2] + text[i + 3];
                string B = "" + text[i + 4] + text[i + 5] + text[i + 6] + text[i + 7];
                string binA = Bin(A);
                string binB = Bin(B);
                for (int j = 0; j < 8; j++) {
                    string f = F(binA, K[j]);
                    string xor = Xor(binB, f);
                    binB = binA;
                    binA = xor;
                }
                for (int j = 23; j >= 0; j--) {
                    string f = F(binA, K[j % 8]);
                    string xor = Xor(binB, f);
                    binB = binA;
                    binA = xor;
                }
                Result += BinToDec(binB) + BinToDec(binA);
            }
            return (Result);
        }

        string Bin(string str) {
            string res = "";
            string s = "";
            for (int i = 0; i < str.Length; i++) {
                s += Convert.ToString((byte)str[i], 2);
                while (s.Length % 8 != 0)
                    s = "0" + s;
                res += s;
                s = "";
            }
            return (res);
        }

        //функция преобразования с циклическим сдвигом
        string F(string Ai, string Xi) {
            
            // Получаем строку суммированную с соответствующим подключем 
            string f = mod2v32(Ai, Xi);
            int z = 0;
            string t;
            string res = "";
            string[] time = new string[32];

            for (int i = 7; i >= 0; i--) {
                // Разбивает f на части по 4 
                t = "" + f[i * 4 + 0] + f[i * 4 + 1] + f[i * 4 + 2] + f[i * 4 + 3];
                t = Convert.ToString(
                    Convert.ToInt32(
                        sbox[z, Convert.ToInt32(t, 2)].ToString(), 16), 2);
                while (t.Length < 4)
                    t = "0" + t;
                res += t;
                z++;
            }
            // Циклический сдвиг на 11 бит
            for (int i = 0; i < 32; i++) {
                time[i] = res[(i + 11) % 32].ToString();
            }
            
            // результат закодированного сообщения собирается в res
            res = "";
            for (int i = 0; i < time.Length; i++) {
                res += time[i];
            }
            return (res);
        }

        //функция сложения по модулю 2^32
        string mod2v32(string a, string x) {
            int k = 0;
            int z = 0;
            string mod = "";
            for (int i = a.Length - 1; i >= 0; i--) {
                z = k + Convert.ToInt32(Convert.ToString(a[i])) + Convert.ToInt32(Convert.ToString(x[i]));
                switch (z){
                    case 0:
                        k = 0;
                        z = 0;
                        break;
                    case 1:
                        k = 0;
                        z = 1;
                        break;
                    case 2:
                        k = 1;
                        z = 0;
                        break;
                    case 3:
                        k = 1;
                        z = 1;
                        break;
                }
                mod = z.ToString() + mod;
            }
            return (mod);
        }

        //функция сложения по модулю 2
        string Xor(string s1, string s2) {
            string res = "";
            for (int i = 0; i < 32; i++) {
                res += (((Int32)s1[i] + (Int32)s2[i]) % 2).ToString();
            }
            return (res);
        }

        //функция преобразования в 16-ую систему
        string BinToDec(string s1) {
            string bbb = "";
            char[] slovo = new char[4];
            string[] res = new string[4];
            int[] aaaa = new int[4];
            for (int i = 0; i < 4; i++) {
                for (int j = 0; j < 8; j++) {
                    res[i] += s1[i * 8 + j];
                }
                slovo[i] = (char)(Convert.ToInt32(res[i], 2));
                bbb += slovo[i];
            }
            return (bbb);

        }

        static void Main(string[] args) {
            var prog = new Program();
            Console.WriteLine("Enter e for encoded? d for decoded: ");
            string check = Console.ReadLine();
            Console.WriteLine(check);

            if (check == "e") {
                // генерирует ключ
                prog.KeyGen();
                
                //создаем поток записи
                FileStream fs = new FileStream("output.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs);

                // функция закодирования
                sw.WriteLine(prog.Crypt());
                // функция выводит ключ в 16-ти рич.системе 
                sw.WriteLine(prog.resKey);
                
                sw.Close();
                

            }
            else if (check == "d") {
                prog.GetKey();
                FileStream fs = new FileStream("input.txt", FileMode.Create, FileAccess.Write);
                //создаем поток записи
                StreamWriter sw = new StreamWriter(fs);

                sw.WriteLine(prog.Decrypt());
                sw.WriteLine(prog.resKey);
                sw.Close();
            }
        }
    }
}
