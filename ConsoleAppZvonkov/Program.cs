using System;
using System.IO;
using System.IO.Compression;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppZvonkov
{
    class Program
    {
        struct Profile
        {
            public string fullName;
            public DateTime birthday;
            public string programmingLanguage;
            public int experience;
            public string mobileNumber;
            public DateTime date;
        }
        struct DescProfile
        {
            public const string fullName = "ФИО";
            public const string birthday = "Дата рождения";
            public const string programmingLanguage = "Любимый язык программирования";
            public const string experience = "Опыт программирования на указанном языке";
            public const string mobileNumber = "Мобильный телефон";
            public const string date = "Анкета заполнена";
        }

        struct QuestionProfile
        {
            public const string fullName = "ФИО";
            public const string birthday = "Дата рождения (Формат ДД.ММ.ГГГГ)";
            public const string programmingLanguage = "Любимый язык программирования (Можно ввести только указанные варианты, иначе ошибка: PHP, JavaScript, C, C++, Java, C#, Python, Ruby)";
            public const string experience = "Опыт программирования на указанном языке (Полных лет)";
            public const string mobileNumber = "Мобильный телефон";
            public const string date = "Анкета заполнена";
        }

        const string dateFormat = "d/M/yyyy";
        const string strSplit = ": ";
        const string bashMessage = "cmd: ";
        const string pathString = @"Анкеты";
        const string helpMessage = @"cmd: -new_profile - Заполнить новую анкету
cmd: -statistics - Показать статистику всех заполненных анкет
cmd: -save - Сохранить заполненную анкету
cmd: -goto_question<Номер вопроса> - Вернуться к указанному вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -goto_prev_question - Вернуться к предыдущему вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -restart_profile - Заполнить анкету заново(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
cmd: -find<Имя файла анкеты> - Найти анкету и показать данные анкеты в консоль
cmd: -delete<Имя файла анкеты> - Удалить указанную анкету
cmd: -list - Показать список названий файлов всех сохранённых анкет
cmd: -list_today - Показать список названий файлов всех сохранённых анкет, созданных сегодня
cmd: -zip<Имя файла анкеты> <Путь для сохранения архива> - Запаковать указанную анкету в архив и сохранить архив по указанному пути
cmd: -help - Показать список доступных команд с описанием
cmd: -exit - Выйти из приложения
";
        static string[] programmingLanguages = new string[] { "PHP", "JavaScript", "C", "C++", "Java", "C#", "Python", "Ruby" };
        static string[] commands = new string[] {
            "-new_profile", // - Заполнить новую анкету
            "-statistics", // - Показать статистику всех заполненных анкет
            "-save", // - Сохранить заполненную анкету
            "-find", // < Имя файла анкеты > -Найти анкету и показать данные анкеты в консоль
            "-delete", // < Имя файла анкеты > -Удалить указанную анкету
            "-list", // - Показать список названий файлов всех сохранённых анкет
            "-list_today", // - Показать список названий файлов всех сохранённых анкет, созданных сегодня
            "-zip", // < Имя файла анкеты > < Путь для сохранения архива > -Запаковать указанную анкету в архив и сохранить архив по указанному пути
            "-help", // - Показать список доступных команд с описанием
            "-print", // Напечатать текущую анкету
            "-exit" // - Выйти из приложения
        };

        static string[] editCommands = new string[] {
            "-goto_question", // < Номер вопроса > -Вернуться к указанному вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
            "-goto_prev_question", // - Вернуться к предыдущему вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
            "-restart_profile", // - Заполнить анкету заново(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
        };

        protected static void myHandler(object sender, ConsoleCancelEventArgs args)
        {
            args.Cancel = true;
        }

        static int GetAge(DateTime birthDate) // вычисляем возраст с учётом даты
        {
            DateTime now = DateTime.Today;
            int age = now.Year - birthDate.Year;
            if (birthDate > now.AddYears(-age)) age--;
            return age;
        }

        static string YearsToString(int year) // переводим число в __лет в правильной форме
        {
            string result = year.ToString();
            if ((year % 100) > 10 && (year % 100) < 15)
                result += " лет";
            else
            {
                switch (year % 10)
                {
                    case 1:
                        result += " год";
                        break;
                    case 2:
                    case 3:
                    case 4:
                        result += " года";
                        break;
                    default:
                        result += " лет";
                        break;
                }
            }
            return result;
        }

        static string MakeFileName(string str) // получаем имя файла для ФИО, по возможности берём имя. Если пусто - возращаем empty.txt
        {
            string result = "";
            char[] splitchar = { ' ' };
            string[] words = str.Trim().Split(splitchar, StringSplitOptions.RemoveEmptyEntries);

            switch (words.Length)
            {
                case 0:
                    result = "empty.txt";
                    break;
                case 1:
                    result = words[0] + ".txt";
                    break;
                default:
                    result = words[1] + ".txt";
                    break;
            }
            return result;
        }

        static void ProfileToZip(string profileName, string pathZip) // сохраняем анкету profileName в zip по указанному пути pathZip
        {
            string fileZip = Path.ChangeExtension(Path.Combine(Path.GetFullPath(pathZip), profileName), "zip");
            string filePath = Path.Combine(Path.GetFullPath(pathString), profileName);
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(pathZip));

            try
            {
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                using (ZipArchive archive = ZipFile.Open(fileZip, ZipArchiveMode.Update))
                {
                    archive.CreateEntryFromFile(filePath, profileName);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Не могу сохранить архив:");
                Console.WriteLine(e.Message);
            }
        }

        static void SaveProfile(Profile profile)
        {
            string nameFile = MakeFileName(profile.fullName);
            string profileFile = Path.Combine(Path.GetFullPath(pathString), nameFile);
            DirectoryInfo dirInfo = new DirectoryInfo(Path.GetFullPath(pathString));
            try
            {
                if (!dirInfo.Exists)
                {
                    dirInfo.Create();
                }
                using (StreamWriter sw = File.CreateText(profileFile))
                {
                    sw.WriteLine(ProfileToString(profile));
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Не могу сохранить файл:");
                Console.WriteLine(e.Message);
            }

        }

        static bool ReadLine(ref Profile profile, string str)
        {
            bool result = true;
            CultureInfo ruRU = new CultureInfo("ru-RU");
            string key, value;
            int position = str.IndexOf(strSplit);
            if (position < 0)
            {
                result = false;
            }
            else
            {
                key = str.Substring(0, position);
                value = str.Substring(position + strSplit.Length);
                switch (key)
                {
                    case DescProfile.fullName:
                        profile.fullName = value;
                        if (value == "")
                            result = false;
                        break;
                    case DescProfile.birthday:
                        result = DateTime.TryParseExact(value, dateFormat, ruRU,
                            DateTimeStyles.None, out profile.birthday);
                        break;
                    case DescProfile.programmingLanguage:
                        if (Array.IndexOf(programmingLanguages, value) == -1)
                        {
                            result = false;
                        }
                        else
                        {
                            profile.programmingLanguage = value;
                        }
                        break;
                    case DescProfile.experience:
                        result = (int.TryParse(value, out profile.experience) && profile.experience > 0);
                        break;
                    case DescProfile.mobileNumber:
                        if (value.Length < 5)
                        {
                            result = false;
                        }
                        else
                        {
                            profile.mobileNumber = value;
                        }
                        break;
                    case DescProfile.date:
                        result = DateTime.TryParseExact(value, dateFormat, ruRU,
                            DateTimeStyles.None, out profile.date);
                        break;
                    default:
                        result = false;
                        break;
                }

            }
            return result;
        }

        static bool LoadProfile(ref Profile profile, string nameFile)
        {
            bool result = false;
            string profileFile = Path.Combine(Path.GetFullPath(pathString), nameFile);
            if (File.Exists(profileFile))
            {
                try
                {
                    using (StreamReader sr = File.OpenText(profileFile))
                    {
                        string str;
                        while ((str = sr.ReadLine()) != null)
                        {
                            if (!ReadLine(ref profile, str))
                            {
                                Console.WriteLine("Ошибка разбора строки {0}", str);
                            }
                            else
                                result = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Не могу прочитать файл:");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Не найден файл: {0}", profileFile);
            }
            return result;
        }

        static string ProfileToString(Profile profile)
        {
            return string.Format("{0}{1}{2}\n", DescProfile.fullName, strSplit, profile.fullName) +
                string.Format("{0}{1}{2}\n", DescProfile.birthday, strSplit, profile.birthday.ToString("d")) +
                string.Format("{0}{1}{2}\n", DescProfile.programmingLanguage, strSplit, profile.programmingLanguage) +
                string.Format("{0}{1}{2}\n", DescProfile.experience, strSplit, profile.experience.ToString()) +
                string.Format("{0}{1}{2}\n", DescProfile.mobileNumber, strSplit, profile.mobileNumber) +
                string.Format("{0}{1}{2}", DescProfile.date, strSplit, profile.date.ToString("d"));
        }
        
        static string QuestionString(int numQuestion) // Возращаем строку с вопросом
        {
            string message;
            switch (numQuestion)
            {
                case 1:
                    message = QuestionProfile.fullName;
                    break;
                case 2:
                    message = QuestionProfile.birthday;
                    break;
                case 3:
                    message = QuestionProfile.programmingLanguage;
                    break;
                case 4:
                    message = QuestionProfile.experience;
                    break;
                case 5:
                    message = QuestionProfile.mobileNumber;
                    break;
                default:
                    message = "";
                    break;
            }
            return message;
        }

        static string MessageString(int numQuestion) // Возвращаем строку с описанием профиля
        {
            string message;
            switch (numQuestion)
            {
                case 1:
                    message = DescProfile.fullName;
                    break;
                case 2:
                    message = DescProfile.birthday;
                    break;
                case 3:
                    message = DescProfile.programmingLanguage;
                    break;
                case 4:
                    message = DescProfile.experience;
                    break;
                case 5:
                    message = DescProfile.mobileNumber;
                    break;
                default:
                    message = "";
                    break;
            }
            return message;
        }
        static Profile InputProfile()
        {
            Profile profile = new Profile();
            int numQuestion = 1;
            int newQuestion;
            do
            {
                string strMessage = MessageString(numQuestion);
                string strQuestion = QuestionString(numQuestion);

                string str = ReadCommand(string.Format("{0}{1}", strQuestion, strSplit) );
                string[] command = str.Split(' ');

                if (Array.IndexOf(editCommands, command[0]) == -1)
                {
                    if (ReadLine(ref profile, strMessage + strSplit + str))
                    {
                        numQuestion++;
                    }
                    else
                    {
                        Console.WriteLine("Введите корректный ответ.");
                    }
                }
                else
                {
                    switch (command[0])
                    {
                        case "-goto_question": //// < Номер вопроса > -Вернуться к указанному вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
                            if (command.Length == 2)
                            {
                                if (int.TryParse(command[1], out newQuestion))
                                {
                                    if (newQuestion >= 1 && newQuestion < numQuestion)
                                    {
                                        numQuestion = newQuestion;
                                    }
                                    else
                                    {
                                        Console.WriteLine("Можно перейти только к предыдущим вопросам. Текущий вопрос {0}.", numQuestion);
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Нужно указать номер вопроса.");
                                }
                            }
                            else
                            {
                                Console.WriteLine("После команды должен идти номер вопроса.");
                            }
                            break;
                        case "-goto_prev_question": // - Вернуться к предыдущему вопросу(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
                            if (numQuestion > 1)
                            {
                                numQuestion--;
                            }
                            else
                            {
                                Console.WriteLine("Это первый вопрос.");
                            }
                            break;
                        case "-restart_profile": // - Заполнить анкету заново(Команда доступна только при заполнении анкеты, вводится вместо ответа на любой вопрос)
                            numQuestion = 1;
                            break;
                    }

                }

            } while (numQuestion <= 5);
            profile.date = DateTime.Now;

            return profile;
        }

        static void ProfilesStatistic()
        {
            Profile profile = new Profile();
            int numProfiles = 0;
            int maxExp = 0;
            int sumAge = 0;
            int avrAge;
            string nameMaxExp = "";
            Dictionary<string, int> langStat = new Dictionary<string, int>();
            int maxLang = 0;
            string popLangName = "";

            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(pathString));
            try
            {
                foreach (var item in dir.GetFiles("*.txt"))
                {
                    if (LoadProfile(ref profile, item.Name))
                    {
                        numProfiles++;
                        sumAge += GetAge(profile.birthday);

                        if (profile.experience > maxExp)
                        {
                            maxExp = profile.experience;
                            nameMaxExp = profile.fullName;
                        }
                        if (!langStat.ContainsKey(profile.programmingLanguage))
                        {
                            langStat.Add(profile.programmingLanguage, 1);
                            if (maxLang == 0)
                            {
                                maxLang = 1;
                                popLangName = profile.programmingLanguage;
                            }
                        }
                        else
                        {
                            langStat[profile.programmingLanguage]++;
                            if (langStat[profile.programmingLanguage] > maxLang)
                            {
                                maxLang = langStat[profile.programmingLanguage];
                                popLangName = profile.programmingLanguage;
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка чтения анкет.");
                Console.WriteLine(e.Message);
            }
            if (numProfiles == 0)
            {
                Console.WriteLine("Анкет не найдено.");
            }
            else
            {
                avrAge = sumAge / numProfiles;
                Console.WriteLine("Средний возраст всех опрошенных: {0} ", YearsToString(avrAge));
                Console.WriteLine("Самый популярный язык: {0}", popLangName);
                Console.WriteLine("Самый опытный программист: {0}", nameMaxExp);
                //Средний возраст всех опрошенных: <Посчитать средний возраст всех тех, кто заполнял анкеты, целое число> (год, года, лет в зависимости от полученного числа, т.е если средний возраст получился 22, то вывести 22 года, если 25, то 25 лет итд)
                //Самый популярный язык программирования: < Название языка программирования, который большинство пользователей указали как любимый >
                //Самый опытный программист: < ФИО человека, у которого указан самый большой опыт работы >
            }
        }

        static void DeleteProfile(string nameFile)
        {
            string profileFile = Path.Combine(Path.GetFullPath(pathString), nameFile);
            if (File.Exists(profileFile))
            {
                try
                {
                    File.Delete(profileFile);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Не могу удалить файл:");
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Не найден файл: {0}", profileFile);
            }

        }

        static void ListProfiles()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(pathString));
            try
            {
                foreach (var item in dir.GetFiles("*.txt"))
                {
                    Console.WriteLine(item.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка получения списка анкет.");
                Console.WriteLine(e.Message);
            }
        }

        static void ListTodayProfiles()
        {
            DirectoryInfo dir = new DirectoryInfo(Path.GetFullPath(pathString));
            try
            {
                foreach (var item in dir.GetFiles("*.txt"))
                {
                    if (item.LastWriteTime.Date == DateTime.Now.Date )
                        Console.WriteLine(item.Name);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка получения списка анкет.");
                Console.WriteLine(e.Message);
            }
        }

        static string ReadCommand(string msg)
        {
            string result = "";
            while (result == "")
            {
                Console.Write(msg);
                result = Console.ReadLine();
                if (result == null)
                {
                    Console.WriteLine("^C");
                    result = "";
                }
            }
            
            return result;
        }

        static void Main(string[] args)
        {
            Console.CancelKeyPress += new ConsoleCancelEventHandler(myHandler);

            Profile profile = new Profile();

            string[] command;

            Console.WriteLine("Выберите действие:");
            do
            {
                command = ReadCommand(bashMessage).Split(' ');
                
                while (Array.IndexOf(commands, command[0]) == -1)
                {
                    Console.WriteLine("Ошибка! Для помощи введите -help");
                    command = ReadCommand(bashMessage).Split(' ');
                }
                switch (command[0])
                {
                    case "-help":
                        Console.WriteLine(helpMessage);
                        break;
                    case "-print":
                        Console.WriteLine(ProfileToString(profile));
                        break;
                    case "-new_profile":
                        profile = InputProfile();
                        break;
                    case "-statistics":
                        ProfilesStatistic();
                        // - Показать статистику всех заполненных анкет
                        break;
                    case "-save":
                        SaveProfile(profile);// - Сохранить заполненную анкету
                        break;
                    case "-find": // < Имя файла анкеты > -Найти анкету и показать данные анкеты в консоль
                        if(command.Length == 2)
                        {
                            if(LoadProfile(ref profile, command[1]))
                                Console.WriteLine(ProfileToString(profile));
                        }
                        else
                        {
                            Console.WriteLine("Нужно указать имя файла!");
                        }
                        
                        break;
                    case "-delete": // < Имя файла анкеты > -Удалить указанную анкету
                        if (command.Length == 2)
                        {
                            DeleteProfile(command[1]);
                        }
                        else
                        {
                            Console.WriteLine("Нужно указать имя файла анкеты.");
                        }
                        break;
                    case "-list": // - Показать список названий файлов всех сохранённых анкет
                        ListProfiles();
                        break;
                    case "-list_today": // - Показать список названий файлов всех сохранённых анкет, созданных сегодня
                        ListTodayProfiles();
                        break;
                    case "-zip": // < Имя файла анкеты > < Путь для сохранения архива > -Запаковать указанную анкету в архив и сохранить архив по указанному пути
                        if(command.Length == 3)
                        {
                            ProfileToZip(command[1], command[2]);
                        }
                        else
                        {
                            Console.WriteLine("Нужно указать имя файла анкеты и путь для сохранения архива.");
                        }
                        break;

                }
            } while (command[0] != "-exit");
        }
    }
}
