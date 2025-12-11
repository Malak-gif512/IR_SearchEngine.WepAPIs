using IR_SearchEngine.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace IR_SearchEngine.Services.Implementations
{
    public class TextProcessor : ITextProcessor
    {
        private readonly HashSet<string> _stopWords = new HashSet<string>
        {
            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any", "are", "aren't", "as", "at",
            "be", "because", "been", "before", "being", "below", "between", "both", "but", "by",
            "can't", "cannot", "could", "couldn't",
            "did", "didn't", "do", "does", "doesn't", "doing", "don't", "down", "during",
            "each",
            "few", "for", "from", "further",
            "had", "hadn't", "has", "hasn't", "have", "haven't", "having", "he", "he'd", "he'll", "he's", "her", "here", "here's", "hers", "herself", "him", "himself", "his", "how", "how's",
            "i", "i'd", "i'll", "i'm", "i've", "if", "in", "into", "is", "isn't", "it", "it's", "its", "itself",
            "let's",
            "me", "more", "most", "mustn't", "my", "myself",
            "no", "nor", "not",
            "of", "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves", "out", "over", "own",
            "same", "shan't", "she", "she'd", "she'll", "she's", "should", "shouldn't", "so", "some", "such",
            "than", "that", "that's", "the", "their", "theirs", "them", "themselves", "then", "there", "there's", "these", "they", "they'd", "they'll", "they're", "they've", "this", "those", "through", "to", "too",
            "under", "until", "up",
            "very",
            "was", "wasn't", "we", "we'd", "we'll", "we're", "we've", "were", "weren't", "what", "what's", "when", "when's", "where", "where's", "which", "while", "who", "who's", "whom", "why", "why's", "with", "won't", "would", "wouldn't",
            "you", "you'd", "you'll", "you're", "you've", "your", "yours", "yourself", "yourselves"
        };

        // الدالة دي بترجع الكلمة + مكانها الأصلي (Position)
        // عشان نستخدمها في الـ Indexing ونحافظ على الترتيب للـ Phrase Search
        public List<(string term, int position)> AnalyzeWithPositions(string text)
        {
            var result = new List<(string, int)>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            // --- 1. Advanced Normalization ---
            string processed = text.ToLower();

            // أ. التعامل مع الملكية (Possessives): user's -> user
            // بنشيل 's اللي في آخر الكلمة
            processed = Regex.Replace(processed, @"['’]s\b", "");

            // ب. التعامل مع الشرط (Hyphens): full-stack -> full stack
            // بنستبدلها بمسافة عشان نفصل الكلمتين عن بعض، مش نلزقهم
            processed = processed.Replace("-", " ");
            processed = processed.Replace("_", " "); // والـ Underscore بالمرة

            // ج. تنظيف شامل: أي حاجة مش (حرف أو رقم أو مسافة) شيلها
            // ده هيشيل النقط، الفواصل، علامات التعجب، والأقواس
            processed = Regex.Replace(processed, @"[^a-z0-9\s]", "");

            // --- 2. Tokenization ---
            // التقطيع بناءً على المسافات
            var rawTokens = processed.Split(new[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

            // --- 3. Processing Loop (Filter & Stem) ---
            // i هنا هو الـ Position الحقيقي للكلمة في الجملة
            int validWordCounter = 0;
            for (int i = 0; i < rawTokens.Length; i++)
            {
                string token = rawTokens[i];

                // Stop Word Removal
                if (_stopWords.Contains(token)) continue;

                // Stemming
                string stemmed = ApplyPorterStemmer(token);

                // 2. بنستخدم العداد الخاص بينا بدل i
                result.Add((stemmed, validWordCounter));
                validWordCounter++;
            }

            return result;
        }

        // الدالة القديمة (Wrapper)
        // بنستخدمها لما نكون عايزين الكلمات بس (زي في SearchService)
        public List<string> Analyze(string text, out List<string> logs)
        {
            logs = new List<string>(); // عشان التوافق مع التوقيع القديم

            // 1. نادي الدالة "العالمية" الجديدة
            var resultWithPositions = AnalyzeWithPositions(text);

            // 2. استخرج الكلمات فقط (Terms) وارمي الـ Positions
            // Select: دي Linq بتعمل Projection
            return resultWithPositions.Select(item => item.term).ToList();
        }
        public string ApplyStemming(string word) => ApplyPorterStemmer(word);

        // --- PORTER STEMMER IMPLEMENTATION (PDF Reference) ---
        private string ApplyPorterStemmer(string word)
        {
            if (word.Length <= 2) return word;

            // Step 1a
            if (word.EndsWith("sses")) word = ReplaceEnd(word, "sses", "ss");
            else if (word.EndsWith("ies")) word = ReplaceEnd(word, "ies", "i");
            else if (word.EndsWith("ss")) { }
            else if (word.EndsWith("s")) word = word.Substring(0, word.Length - 1);

            // Step 1b
            bool extraStep = false;
            if (word.EndsWith("eed"))
            {
                string stem = word.Substring(0, word.Length - 3);
                if (GetMeasure(stem) > 0) word = stem + "ee";
            }
            else if (word.EndsWith("ed"))
            {
                string stem = word.Substring(0, word.Length - 2);
                if (ContainsVowel(stem)) { word = stem; extraStep = true; }
            }
            else if (word.EndsWith("ing"))
            {
                string stem = word.Substring(0, word.Length - 3);
                if (ContainsVowel(stem)) { word = stem; extraStep = true; }
            }

            if (extraStep)
            {
                if (word.EndsWith("at") || word.EndsWith("bl") || word.EndsWith("iz")) word += "e";
                else if (EndsWithDoubleConsonant(word) && !word.EndsWith("l") && !word.EndsWith("s") && !word.EndsWith("z"))
                    word = word.Substring(0, word.Length - 1);
                else if (GetMeasure(word) == 1 && EndsWithCVC(word)) word += "e";
            }

            // Step 1c
            if (word.EndsWith("y") && ContainsVowel(word.Substring(0, word.Length - 1)))
                word = word.Substring(0, word.Length - 1) + "i";

            // Step 2
            if (GetMeasure(word) > 0)
            {
                if (word.EndsWith("ational")) ReplaceIfMeasure(ref word, "ational", "ate", 0);
                else if (word.EndsWith("tional")) ReplaceIfMeasure(ref word, "tional", "tion", 0);
                else if (word.EndsWith("enci")) ReplaceIfMeasure(ref word, "enci", "ence", 0);
                else if (word.EndsWith("anci")) ReplaceIfMeasure(ref word, "anci", "ance", 0);
                else if (word.EndsWith("izer")) ReplaceIfMeasure(ref word, "izer", "ize", 0);
                else if (word.EndsWith("abli")) ReplaceIfMeasure(ref word, "abli", "able", 0);
                else if (word.EndsWith("alli")) ReplaceIfMeasure(ref word, "alli", "al", 0);
                else if (word.EndsWith("entli")) ReplaceIfMeasure(ref word, "entli", "ent", 0);
                else if (word.EndsWith("eli")) ReplaceIfMeasure(ref word, "eli", "e", 0);
                else if (word.EndsWith("ousli")) ReplaceIfMeasure(ref word, "ousli", "ous", 0);
                else if (word.EndsWith("ization")) ReplaceIfMeasure(ref word, "ization", "ize", 0);
                else if (word.EndsWith("ation")) ReplaceIfMeasure(ref word, "ation", "ate", 0);
                else if (word.EndsWith("ator")) ReplaceIfMeasure(ref word, "ator", "ate", 0);
                else if (word.EndsWith("alism")) ReplaceIfMeasure(ref word, "alism", "al", 0);
                else if (word.EndsWith("iveness")) ReplaceIfMeasure(ref word, "iveness", "ive", 0);
                else if (word.EndsWith("fulness")) ReplaceIfMeasure(ref word, "fulness", "ful", 0);
                else if (word.EndsWith("ousness")) ReplaceIfMeasure(ref word, "ousness", "ous", 0);
                else if (word.EndsWith("aliti")) ReplaceIfMeasure(ref word, "aliti", "al", 0);
                else if (word.EndsWith("iviti")) ReplaceIfMeasure(ref word, "iviti", "ive", 0);
                else if (word.EndsWith("biliti")) ReplaceIfMeasure(ref word, "biliti", "ble", 0);
            }

            // Step 3
            if (GetMeasure(word) > 0)
            {
                if (word.EndsWith("icate")) ReplaceIfMeasure(ref word, "icate", "ic", 0);
                else if (word.EndsWith("ative")) ReplaceIfMeasure(ref word, "ative", "", 0);
                else if (word.EndsWith("alize")) ReplaceIfMeasure(ref word, "alize", "al", 0);
                else if (word.EndsWith("iciti")) ReplaceIfMeasure(ref word, "iciti", "ic", 0);
                else if (word.EndsWith("ical")) ReplaceIfMeasure(ref word, "ical", "ic", 0);
                else if (word.EndsWith("ful")) ReplaceIfMeasure(ref word, "ful", "", 0);
                else if (word.EndsWith("ness")) ReplaceIfMeasure(ref word, "ness", "", 0);
            }

            // Step 4
            if (GetMeasure(word) > 1) // Needs m > 1
            {
                if (word.EndsWith("al")) ReplaceIfMeasure(ref word, "al", "", 1);
                else if (word.EndsWith("ance")) ReplaceIfMeasure(ref word, "ance", "", 1);
                else if (word.EndsWith("ence")) ReplaceIfMeasure(ref word, "ence", "", 1);
                else if (word.EndsWith("er")) ReplaceIfMeasure(ref word, "er", "", 1);
                else if (word.EndsWith("ic")) ReplaceIfMeasure(ref word, "ic", "", 1);
                else if (word.EndsWith("able")) ReplaceIfMeasure(ref word, "able", "", 1);
                else if (word.EndsWith("ible")) ReplaceIfMeasure(ref word, "ible", "", 1);
                else if (word.EndsWith("ant")) ReplaceIfMeasure(ref word, "ant", "", 1);
                else if (word.EndsWith("ement")) ReplaceIfMeasure(ref word, "ement", "", 1);
                else if (word.EndsWith("ment")) ReplaceIfMeasure(ref word, "ment", "", 1);
                else if (word.EndsWith("ent")) ReplaceIfMeasure(ref word, "ent", "", 1);
                else if (word.EndsWith("ion"))
                {
                    string stem = word.Substring(0, word.Length - 3);
                    if (GetMeasure(stem) > 1 && (stem.EndsWith("s") || stem.EndsWith("t"))) word = stem;
                }
                else if (word.EndsWith("ou")) ReplaceIfMeasure(ref word, "ou", "", 1);
                else if (word.EndsWith("ism")) ReplaceIfMeasure(ref word, "ism", "", 1);
                else if (word.EndsWith("ate")) ReplaceIfMeasure(ref word, "ate", "", 1);
                else if (word.EndsWith("iti")) ReplaceIfMeasure(ref word, "iti", "", 1);
                else if (word.EndsWith("ous")) ReplaceIfMeasure(ref word, "ous", "", 1);
                else if (word.EndsWith("ive")) ReplaceIfMeasure(ref word, "ive", "", 1);
                else if (word.EndsWith("ize")) ReplaceIfMeasure(ref word, "ize", "", 1);
            }

            // Step 5a
            if (word.EndsWith("e"))
            {
                string stem = word.Substring(0, word.Length - 1);
                int m = GetMeasure(stem);
                if (m > 1 || (m == 1 && !EndsWithCVC(stem))) word = stem;
            }

            // Step 5b
            if (GetMeasure(word) > 1 && EndsWithDoubleConsonant(word) && word.EndsWith("l"))
                word = word.Substring(0, word.Length - 1);

            return word;
        }

        // --- Helpers ---
        private void ReplaceIfMeasure(ref string word, string suffix, string replacement, int minMeasure)
        {
            if (word.EndsWith(suffix))
            {
                string stem = word.Substring(0, word.Length - suffix.Length);
                if (GetMeasure(stem) > minMeasure) word = stem + replacement;
            }
        }
        private bool IsConsonant(string str, int i)
        {
            if ("aeiou".Contains(str[i])) return false;
            if (str[i] == 'y') return i == 0 ? true : !IsConsonant(str, i - 1);
            return true;
        }
        private int GetMeasure(string str)
        {
            int m = 0, i = 0;
            while (i < str.Length && !IsConsonant(str, i)) i++;
            while (i < str.Length)
            {
                while (i < str.Length && IsConsonant(str, i)) i++;
                if (i >= str.Length) break;
                while (i < str.Length && !IsConsonant(str, i)) i++;
                m++;
            }
            return m;
        }
        private bool ContainsVowel(string str) { for (int i = 0; i < str.Length; i++) if (!IsConsonant(str, i)) return true; return false; }
        private bool EndsWithDoubleConsonant(string str) => str.Length >= 2 && str[str.Length - 1] == str[str.Length - 2] && IsConsonant(str, str.Length - 1);
        private bool EndsWithCVC(string str)
        {
            if (str.Length < 3) return false;
            return IsConsonant(str, str.Length - 1) && !IsConsonant(str, str.Length - 2) && IsConsonant(str, str.Length - 3) && !"wxy".Contains(str[str.Length - 1]);
        }
        private string ReplaceEnd(string word, string suffix, string replacement) => word.Substring(0, word.Length - suffix.Length) + replacement;
    }
}