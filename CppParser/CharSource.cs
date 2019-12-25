using System;
using System.Linq;

namespace CppParser
{
    public class CharSource
    {
        private System.Collections.Generic.Stack<char> pushed_char = new System.Collections.Generic.Stack<char>();
        private System.Collections.Generic.LinkedList<char> returned_char = new System.Collections.Generic.LinkedList<char>();
        private System.IO.Stream @in;
        private int nchar;          // Number of characters read
        private int newlines;       // Count encountered newlines

        /**
         * Maximum number of characters that can be pushed back, with
         * get_before() still returning a valid value (not 0).
         * Without this we must maintain a returned_char queue with
         * a size equal to the file read.
         */
        private const int MAX_REWIND = 10;

        public CharSource(System.IO.Stream s = null) {
            this.@in = s;
            this.nchar = 0;
            this.newlines = 0;
        }

        /*
         * Obtain the next valid character from the source.
         * Ignore (silently consume without returning them) non-ASCII characters
         * This is required, because these cannot be correctly classified
         * as valid identifier parts (in languages that allow them)
         * without knowledge of the character encoding scheme in effect.
         * On EOF return false and set c to 0.
         */
        public bool get(ref char c)
        {
            if (pushed_char.Count == 0)
            {
                // Read, ignoring non ASCII-characters
                do
                {
                    var b = this.@in.ReadByte();
                    if (b == -1)
                        {
                            c = (char)0;
                            return false;
                        }
                        else
                        {
                            c = (char)b;
                        }
                    nchar++;
                } while (c < 0 || c > 127);
                if (c == '\n')
                    newlines++;
            }
            else
            {
                c = pushed_char.Pop();
            }
            returned_char.AddLast(c);
            while (returned_char.Count > MAX_REWIND)
                returned_char.RemoveFirst();
            return true;
        }

        /**
         * Return current line number
         */
        public int line_number() { return newlines + 1; }

        /**
         * Return the next character from source without removing it
         * Return 0 on EOF.
         */
        public char char_after()
        {
            char c = (char)0;
            if (get(ref c))
            {
                push(ref c);
                return c;
            }
            else
                return (char)0;
        }

        /**
         * Return the nth character before the one returned
         * Return 0 if no such character is available.
         */
        public char char_before(int n = 1)
        {
            int index = returned_char.Count - n - 1;

            if (index >= 0)
                return returned_char.ElementAt(index);
            else
                return (char)0;
        }

        /** Return number of characters read */
        public int get_nchar() { return nchar - pushed_char.Count; }

        /**
         * Push the specified character back into the source
         * In effect, this is an undo of the last get, and therefore
         * also moves back one character in the returned character queue.
         * */
        public void push(ref char c)
        {
            pushed_char.Push(c);
            if (returned_char.Count > 0)
                returned_char.RemoveLast();
        }
    }
}
