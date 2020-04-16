using diploma.Data;
using diploma.Data.Entities;
using diploma.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore.Internal;
using Newtonsoft.Json;
using Spire.Doc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace diploma.Controllers
{
    [Authorize(Roles = "file_admin")]
    public class DocFileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public DocFileController(UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _env = env;
        }

        public IActionResult Index()
        {
            using ApplicationDbContext db = AppContextFactory.DB;
            var docs = db.DocFiles.ToList();
            return View(docs);
        }

        public IActionResult Create() => View(new DocFileLoadViewModel());

        [HttpPost]
        public async Task<IActionResult> Create(string name)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }

            // Сохранение данных файла (сам файл не сохраняется ибо он в целом то и не нужен).
            var model = new DocFileLoadViewModel();
            await TryUpdateModelAsync<DocFileLoadViewModel>(model, "Document", i => i.Document);

            if (model.Document == null) 
            {
                ModelState.AddModelError("File", "Загрузите файл!");
                return View();
            }

            // Проверка, что файл дейстительно вордовский, а иначе хлоп его.
            var extension = Path.GetExtension(model.Document.FileName);
            var allowedExtensions = new string[] { ".docx", ".doc" };

            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("WrongFileExtension", "Добавленный файл имеет отличное от .doc, .docx расширение!");
                return View();
            }

            // Достали из глубоких штанов пользователя и ...
            var user = await _userManager.GetUserAsync(User);

            // ... потные от волнения обрабатываем файл.
            // Сначала сохраняем файл. Получаем путь, где он лежит теперь и ничего не делает.
            string path = await DocFileHelper.SaveFile(_env, model.Document);

            // Получаем хеш файла.
            string fileHash = DocFileHelper.ComputeHash(_env, path);

            // А если такой файл уже есть, то удаляем его.
            using var db = AppContextFactory.DB;
            var exist = db.DocFiles.Any(i => i.FileHash == fileHash);
            if (exist)
            {
                string deletePath = Path.Combine(_env.WebRootPath, path);
                System.IO.File.Delete(deletePath);

                ModelState.AddModelError("File already exists", "Такой файл уже ранее был загружен!");
                return View();
            }

            // Забираем текст из файла для работы mystem.
            string text = DocFileHelper.GetText(_env, path);

            // Сохраняем текст во времяночку, чтобы mystem смог получить на вход файл.
            DocFileHelper.SaveToTmpTextFile(_env, text);

            // Обрабатываем файл mystem и генерируем выходной файл.
            DocFileHelper.RunMystem(_env);

            // Читаем данные прогона.
            var result = DocFileHelper.ReadResult(_env);

            // Фильтруем немного базар...
            var filteredWords = DocFileHelper.FilterWords(result);

            // Выдираем данные об исполнителе.
            var employee = DocFileHelper.GetPersonalData(_env, path);

            // И сохраняем...
            DocFileHelper.SaveResult(filteredWords, fileHash, path, user, employee);

            return View(name);
        }

        public IActionResult Details(int id)
        {
            using var db = AppContextFactory.DB;
            var doc = db.DocFiles.First(i => i.Id == id);
            var words = db.Words.Where(i => i.DocFileId == id).ToList();

            var model = new DocFileDetailsViewModel() { Document = doc, Words = new List<DocFileDetailsItem>() };

            foreach (var item in words)
            {
                var selectedList = new List<SelectListItem>() { new SelectListItem() { Value = "0", Text = "[Выберите отношение]" } };
                selectedList.AddRange(
                    from f in db.Facets
                    where new string[] { "skills", "subjects" }.Contains(f.Code)
                    join fi in db.FacetItems on f.Id equals fi.Facet.Id
                    select new SelectListItem() { Text = fi.Name, Value = fi.Id.ToString() }
                );

                if (item.FacetItemId.HasValue)
                {
                    var val = item.FacetItemId.Value.ToString();
                    selectedList.First(i => i.Value == val).Selected = true;
                }

                model.Words.Add(new DocFileDetailsItem() { Word = item, Types = selectedList });
            }

            model.Words = model.Words.OrderByDescending(i => i.Word.HasMeaning && i.Word.FacetItemId.HasValue).ToList();

            return View(model);
        }

        /// <summary>
        /// Класс, описывающий данные из формы на страничке.
        /// Стрингами, потому что лень шить конвертер в int32. =)
        /// </summary>
        public class DetailsPageData
        {
            public string wordId { get; set; }
            public string selectId { get; set; }
        }

        [HttpPost]
        public async Task<JsonResult> UpdateWords([FromBody] List<DetailsPageData> data)
        {
            using var db = AppContextFactory.DB;
            using var t = db.Database.BeginTransaction();

            try
            {
                // парционирование, чтобы не мучать базу данных.
                int portion = 0;

                foreach (var item in data)
                {
                    int wordId, typeId;
                    if (int.TryParse(item.wordId, out wordId) && int.TryParse(item.selectId, out typeId))
                    {
                        var word = db.Words.First(i => i.Id == wordId);
                        word.FacetItemId = typeId;
                    }

                    portion++;

                    if (portion % 10 == 0)
                    {
                        await db.SaveChangesAsync();
                    }
                }

                // Остатки сохраняем.
                await db.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex) 
            {
                await t.RollbackAsync();
                return Json(new { error = true, message = ex.Message });
            }

            return Json(new { error = false, message = "саксесс" });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            using (var db = AppContextFactory.DB)
            {
                var t = db.Database.BeginTransaction();
                try
                {
                    var f = db.DocFiles.FirstOrDefault(i => i.Id == id);
                    db.DocFiles.Remove(f);
                    await db.SaveChangesAsync();
                    await t.CommitAsync();
                }
                catch (Exception ex)
                {
                    await t.RollbackAsync();
                    ModelState.AddModelError("Error", string.Format("Что-то завязано, нельзя удалить файл! {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message));
                    return View();
                }
            }

            return RedirectToAction("Index");
        }
    }

    /// <summary>
    /// Класс с вспомогательными методами для контроллера по управлению файлами.
    /// </summary>
    public static class DocFileHelper
    {
        public static string CreateUniqueName(IFormFile file)
        {
            return Guid.NewGuid().ToString() + "_" + file.FileName;
        }

        /// <summary>
        /// Сохранение файла на сервер и получение пути до него.
        /// </summary>
        public async static Task<string> SaveFile(IWebHostEnvironment env, IFormFile file)
        {
            string path = @"Files/Documents";
            string uniqueName = CreateUniqueName(file);
            string directoryPath = Path.Combine(env.WebRootPath, path);

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(env.WebRootPath, path, uniqueName);

            using FileStream fs = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(fs);

            return Path.Combine(path, uniqueName);
        }

        public static string GetText(IWebHostEnvironment env, string fileName)
        {
            string path = Path.Combine(env.WebRootPath, fileName);

            Document document = new Document();
            document.LoadFromFile(path);

            return document.GetText();
        }

        /// <summary>
        /// Рассчет хеша файла.
        /// </summary>
        public static string ComputeHash(IWebHostEnvironment env, string fileName)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(Path.Combine(env.WebRootPath, fileName));

            var hash = md5.ComputeHash(stream);

            return BitConverter.ToString(hash);
        }

        /// <summary>
        /// Сохранение файла в текстовый файл.
        /// </summary>
        public static void SaveToTmpTextFile(IWebHostEnvironment env, string text, string inputPath = @"input.txt")
        {
            string tmpFilePath = Path.Combine(env.WebRootPath, inputPath);
            File.WriteAllText(tmpFilePath, text);
        }

        /// <summary>
        /// Отработка файла mystem-ом.
        /// </summary>
        public static void RunMystem(IWebHostEnvironment env, string inputPath = @"input.txt", string outputPath = @"output.txt")
        {
            inputPath = Path.Combine(env.WebRootPath, inputPath);
            outputPath = Path.Combine(env.WebRootPath, outputPath);

            Process process = new Process();
            process.StartInfo.FileName = "mystem.exe";
            process.StartInfo.Arguments = string.Format("-cgin --format json {0} {1}", inputPath, outputPath);
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();
        }

        public static List<MystemResult> ReadResult(IWebHostEnvironment env, string outputPath = @"output.txt")
        {
            int c = 0, toSkip = 22; // потому что библиотека одна в триальной версии и добавляет хрень в файл.
            List<MystemResult> words = new List<MystemResult>();

            string path = Path.Combine(env.WebRootPath, outputPath);

            using (var stream = new StreamReader(path))
            {
                while (stream.Peek() >= 0)
                {
                    if (c <= toSkip)
                    {
                        c++;
                        continue;
                    }

                    string jsonString = stream.ReadLine();
                    var result = JsonConvert.DeserializeObject<MystemResult>(jsonString);

                    // Распознаем, что это слово, тогда добавляем данные о нем.
                    if (result.analysis != null && result.analysis.Count > 0)
                    {
                        words.Add(result);
                    }
                }
            }

            return words;
        }

        /// <summary>
        /// Фильтрует базар! в тексте, помечая не значимые слова.
        /// </summary>
        /// <param name="words"></param>
        public static List<MystemResult> FilterWords(List<MystemResult> words, string stopPath = @"stopwords.txt")
        {
            List<string> stopWords = new List<string>();

            using (var sr = new StreamReader(stopPath))
            {
                string w = string.Empty;
                while (sr.Peek() >= 0)
                {
                    w = sr.ReadLine();
                    stopWords.Add(w.ToLower());
                }
            }

            // Удаление слов из перечня.
            foreach (var word in words)
            {
                // берем только первую лексему.
                var lex = word.analysis[0].lex;
                word.meaningfull = stopWords.Contains(lex) ? false : true;
            }

            return words;
        }

        /// <summary>
        /// Сохранение результата.
        /// </summary>
        public static string SaveResult(List<MystemResult> words, string fileHash, string fileName, ApplicationUser user, PersonalData employee)
        {
            using (var db = AppContextFactory.DB)
            {
                using var t = db.Database.BeginTransaction();
                try
                {
                    DocFile doc = new DocFile()
                    {
                        ApplicationUserId = user.Id,
                        FileHash = fileHash,
                        FIO = employee != null ? employee.Name : string.Empty,
                        Skills = employee != null ? employee.Info : string.Empty,
                        WorkPlace = string.Empty,
                        FileName = fileName
                    };

                    db.DocFiles.Add(doc);
                    db.SaveChanges();

                    // Сохраняем слова, имея id-шник файла.
                    int portion = 0;

                    // Слова берутся единожды, считается их частота встречаемости.
                    words = words.Where(i => i.meaningfull).ToList();

                    // Удаляем странные буквы.
                    words = words.Where(i => i.text.Length > 1).ToList();

                    // Считаем частотность встречаемсти в тексте.
                    var frequency = from w in words
                                    group w by w.analysis[0].lex into grp
                                    select new { word = grp.Key, frequency = grp.Count() };

                    // делаем distinct.
                    var distinctWords = (
                        from f in frequency
                        join w in words on f.word equals w.analysis[0].lex
                        select new MystemResult () { text = w.text, analysis = w.analysis, frequency = f.frequency, meaningfull = true }
                    ).Distinct();

                    // Берутся только значимые слова.
                    foreach (var word in distinctWords)
                    {
                        int index = !string.IsNullOrEmpty(word.analysis[0].gr) ? word.analysis[0].gr.IndexOf(",") : -1;

                        Word w = new Word()
                        {
                            DocFileId = doc.Id,
                            HasMeaning = true, // по-умолчанию все значимы, по мере догрузки документов эта характеристика будет меняться.
                            InitialForm = word.analysis[0].lex, // берем первый вариант слова.
                            MystemData = JsonConvert.SerializeObject(word),
                            TextVersion = word.text,
                            Frequency = word.frequency
                        };

                        db.Words.Add(w);

                        // Сохраняем порциями, чтобы БД не грузить особо.
                        if (portion % 10 == 0)
                        {
                            db.SaveChanges();
                        }

                        portion++;
                    }

                    db.SaveChanges();
                    t.Commit();
                }
                catch (Exception ex)
                {
                    t.Rollback();
                    return ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                }
            }

            return string.Empty;
        }

        public class PersonalData 
        {
            public string Name { get; set; }
            public string Info { get; set; }
        }

        /// <summary>
        /// Выборка данных ФИО и места работы.
        /// </summary>
        public static PersonalData GetPersonalData(IWebHostEnvironment env, string path)
        {
            string fullPath = Path.Combine(env.WebRootPath, path);
            
            Document doc = new Document();
            doc.LoadFromFile(fullPath);

            string text = doc.GetText();
            string[] lines = text.Replace("\r", "").Split("\n");

            var filtered = lines.Where(i => i.ToLower().Contains("исполнитель:")).ToList();

            if (!(filtered.Count > 1 && filtered.Count == 0))
            {
                // Берем и следующую строку на всякий.
                int index = lines.ToList().FindIndex(HasString);

                // Опасное место, может быть выход за границы.
                string s = string.Concat(lines[index], lines[index + 1]);

                // Пример строки.
                // Исполнитель: Семенова Н.В., Министр образования и науки Ульяновской областиВсе исполнители по документу:
                string[] splittedLine = s.Split(',');

                // Скорее всего объединение строк не принесло результата.
                if (splittedLine.Length <= 2) {
                    splittedLine = lines[index].Split(',');
                }

                // Убираем пробелы между словами.
                splittedLine = splittedLine.Select(i => i.Trim()).ToArray();

                index = splittedLine[0].IndexOf(":");

                string name = splittedLine[0].Substring(index + 1).Trim();
                string info = string.Empty;

                if (splittedLine.Length > 2)
                {
                    info = splittedLine[1] + "|" + splittedLine[2];
                }
                else if (splittedLine.Length > 1) {
                    info = splittedLine[1];
                }

                return new PersonalData() { Info = info, Name = name };
            }

            return null;
        }

        private static bool HasString(string s) 
        {
            if (s.ToLower().Contains("исполнитель:"))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }

    [Serializable]
    public class Data
    {
        public string lex { get; set; }
        public string gr { get; set; }
    }

    [Serializable]
    public class MystemResult: IEquatable<MystemResult>
    {
        public List<Data> analysis { get; set; }
        public string text { get; set; }
        public bool meaningfull { get; set; }
        public int frequency { get; set; }

        // Немного костыльненко, но вроде надежно прибил.
        public bool Equals([AllowNull] MystemResult other)
        {
            if (Object.Equals(this, other))
            {
                return true;
            }
            else 
            {
                if (analysis[0].lex == other.analysis[0].lex)
                {
                    return true;
                }
                else 
                {
                    return false;
                }
            }
        }

        public override int GetHashCode()
        {
            return analysis[0].lex.GetHashCode();
        }
    }
}