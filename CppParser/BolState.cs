using System;
namespace CppParser
{
    public class BolState
    {
        private bool bol_state;
        private bool bol_space_state;
        private int indentation;

        public BolState() { saw_newline(); }

        /** True at the beginning of a line */
        public bool at_bol() { return bol_state; }

        /** True at the beginning of a line, possibly with spaces */
        public bool at_bol_space() { return bol_space_state; }

        /** Return the current line's indentation. */
        public int get_indentation() { return indentation; }

        /** Called when processing a newline */
        public void saw_newline()
        {
            bol_state = true;
            bol_space_state = true;
            indentation = 0;
        }

        /** Called when processing a space character c0 */
        public void saw_space(int c)
        {
            bol_state = false;
            if (bol_space_state)
            {
                if (c == ' ')
                    indentation++;
                /*
                 * 0 8
                 * 1 8
                 * ...
                 * 7 8
                 * 8 16
                 * ...
                 */
                else if (c == '\t')
                    indentation = (indentation / 8 + 1) * 8;
            }
        }

        /** Called when processing a non-space character */
        public void saw_non_space() { bol_state = bol_space_state = false; }
    }
}
