using System;
using System.IO;


namespace Blue3DPrinter
{
    public enum EcfParseResult
    {
        /// <summary>
        /// exit
        /// </summary>
        EOF = -2,
        /// <summary>
        /// an error was generated
        /// </summary>
        ERROR = -1,
        /// <summary>
        /// return a valid property
        /// </summary>
        SUCCESS = 0,
        /// <summary>
        /// The '{' bracket
        /// </summary>
        BEGINLEVEL = 1,
        /// <summary>
        /// The '}' bracket
        /// </summary>
        ENDLEVEL = 2,
    }


    public abstract class BaseParser
    {
        protected static char[] charsToTrim = { ' ', '\t', '\'' };
        protected static char[] charsToSplit = { '\"', ',' };

        /// <summary>
        /// current line number readed in file, useful for debuging
        /// </summary>
        public int CurrentLine { get; protected set; }
        /// <summary>
        /// current line can contain multiple objects
        /// </summary>
        protected string line;
        /// <summary>
        /// Length of currentLine
        /// </summary>
        protected int length;
        /// <summary>
        /// cursor's position in the currentLine
        /// </summary>
        protected int position = 0;
        /// <summary>
        /// Remove whitespace and null char '\0', yes, i found also null char...
        /// </summary>
        /// <returns>return false if reach end of file</returns>
        protected bool advanceToValidChar()
        {
            // IsWhiteSpace is better than check the specific char: ' '
            while (position < length && (char.IsWhiteSpace(line[position]) || line[position] == '\0')) position++;
            return position < length;
        }

        /// <summary>
        /// Get a new line, clear whitespace and remove comments
        /// Implemented two types of comments: the row comment start with "#" and section comment enclosed between "/* and "*/"
        /// </summary>
        /// <returns>return false if it can't find a valid line, stop reading file</returns>
        protected bool getNextValidLine(StreamReader file)
        {
            line = null;
            CurrentLine++;
            bool requireCommentSectionEnd = false;

            while ((line = TrimLine(file.ReadLine(), ref requireCommentSectionEnd)) == null && !file.EndOfStream) { CurrentLine++; }
            
            if (line != null) { position = 0; length = line.Length; }

            if (line == null && requireCommentSectionEnd)
                Console.WriteLine("# error: the comment section never end");

            return line != null;
        }
        /// <summary>
        /// Remove start and end white space and skip chars after comment's "#"
        /// </summary>
        /// <returns>return null if string is empty</returns>
        /// <param name="requireCommentSectionEnd">if found the start of a big comment section with "/*", require to seach untill end "*/"</param>
        public static string TrimLine(string str, ref bool requireCommentSectionEnd)
        {
            if (string.IsNullOrEmpty(str)) return null;
            str.Replace("\0", string.Empty);
            if (string.IsNullOrEmpty(str)) return null;

            int length = str.Length;
            int i = 0;
            int begin = 0;
            int end = 0;

            //if a section comment has begun, read until its end
            if (requireCommentSectionEnd)
            {
                for (begin = 0; begin < length - 1; begin++)
                {
                    if (str[begin] == '*' && str[begin + 1] == '/')
                    {
                        requireCommentSectionEnd = false;
                        break;
                    }
                }
                begin += 2;
            }

            //find end
            for (end = begin; end < length; end++)
            {
                if (str[end] == '#')
                {
                    break;
                }
                //find start of comment section
                if (!requireCommentSectionEnd && end < (length - 1) && str[end] == '/' && str[end + 1] == '*')
                {
                    requireCommentSectionEnd = true;
                    break;
                }

            }
            //end of comment section is inside same line but the next chars will be not considered
            if (requireCommentSectionEnd)
                for (i = end; i < length - 1; i++)
                    if (str[i] == '*' && str[i + 1] == '/')
                    {
                        requireCommentSectionEnd = false;
                        break;
                    }

            //trim start
            while (begin < end && char.IsWhiteSpace(str[begin])) begin++;

            //trim end (index end is the next index !)
            while (end > begin && char.IsWhiteSpace(str[end - 1])) end--;

            length = end - begin;
            if (length <= 0) return null;

            return str.Substring(begin, length);
        }

    }
    /// <summary>
    /// A tool to read the ecf text. Not safety check, if something wrong not genereate an error
    /// </summary>
    public class EcfParser : BaseParser
    {
        int level = 0;

        public class PropPair
        {
            /// <summary>
            /// Exist always, if null mean the proppair is null
            /// </summary>
            public string PropKey;
            /// <summary>
            /// it may be null if not found or if an error occour. Can be a string or and array of strings
            /// </summary>
            public object PropValue;

            public override string ToString()
            {
                if (PropKey != null)
                {
                    if (PropValue != null)
                    {
                        if (PropValue.GetType().IsArray)
                        {
                            string[] propValue = (string[])PropValue;
                            return PropKey + " : array.length=" + propValue.Length;
                        }
                        else if (PropValue is string)
                        {
                            string propValue = (string)PropValue;
                            return PropKey + " : " + propValue;
                        }
                        else
                        {
                            return PropKey + " : " + PropValue.GetType().ToString();
                        }
                    }
                    return PropKey + " : null";
                }
                else
                {
                    return "ERROR KEY CANT BE NULL";
                }
            }
        }

        /// <summary>
        /// the new level start at "{" and end at "}". the root level is 0, so the first block description is at level 1
        /// </summary>
        public int CurrentLevel => level;

        /// <summary>
        /// if return false, stop the reading because there is an error or it reach the eof
        /// </summary>
        /// <returns>return < 0 if fail, if it returns the begin and end level the propPair is null </returns>
        public EcfParseResult GetNextPropertyPair(StreamReader file, out PropPair propPair)
        {
            propPair = null;

            //what ??
            if (level < 0) return EcfParseResult.ERROR;

            if (position >= length || line == null)
                if (!getNextValidLine(file))
                    return EcfParseResult.EOF;

            if (!advanceToValidChar()) return EcfParseResult.ERROR;

            // begin or end of a level, usually only one level, but there are also nested levels (groups)
            switch (line[position])
            {
                case '{':
                    level++; position++;
                    return EcfParseResult.BEGINLEVEL;

                case '}':
                    level--; position++;
                    return EcfParseResult.ENDLEVEL;
            }

            propPair = new PropPair();
            
            propPair.PropKey = (string)readValue();

            propPair.PropValue = readValue();

            return EcfParseResult.SUCCESS;
        }

        /// <summary>
        /// A string or a string array
        /// </summary>
        /// <returns></returns>
        object readValue()
        {
            //ignore start white spaces
            if (!advanceToValidChar()) return null;
           
            int start = position;


            // is a begin of string: the comma inside a string not define a new object but it's the separator of the array's elements
            if (line[position] == '\"')
            {
                position++;
                bool hascomma = false;
                //read untill '"' (or end of line if something wrong)
                while (line[position] != '\"' && position < length)
                {
                    hascomma |= line[position] == ',';
                    position++;
                }
                if (line[position] == '\"') position++;
                
                string value = line.Substring(start, position - start);
                value = value.TrimEnd();
                position++;

                if (hascomma)
                {
                    string[] values = value.Split(charsToSplit, StringSplitOptions.RemoveEmptyEntries);

                    for (int i = 0; i < values.Length; i++)
                    {
                        values[i] = values[i].TrimStart();
                        values[i] = values[i].TrimEnd();
                    }
                    return values;
                }
                else
                {
                    return value;
                }
            }
            else
            {
                //read untill ',' ':' '"' or end of line
                while (position < length && line[position] != ',' && line[position] != ':' && line[position] != '\"') position++;
                string value = line.Substring(start, position - start);
                value = value.TrimEnd();
                position++;
                return value;
            }
        }

    }
}
