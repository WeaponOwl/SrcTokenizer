using System;
namespace CppParser
{
    public class SymbolTable
    {
        int next_symbol_value;
        System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>> table;

        /** Construct an empty symbol table */
        public SymbolTable()
        {
            this.next_symbol_value = (int)TokenId.IDENTIFIER;
            this.table = new System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, int>>() { new System.Collections.Generic.Dictionary<string, int>() };
        }

        /** Return a symbol's value, adding it if needed */
        public int value(string symbol)
        {
            System.Collections.Generic.Dictionary<string, int> current_map = table[table.Count - 1];
            if (current_map.ContainsKey(symbol)) return current_map[symbol];

            foreach (var i in table)
            {
                if (i.ContainsKey(symbol)) return i[symbol];
            }

            // Not found; insert it the the current scope
            int val = next_symbol_value++;
            current_map.Add(symbol, val);
            return val;
        }
        public void enter_scope()
        {
            table.Add(new System.Collections.Generic.Dictionary<string, int>());
        }
        public void exit_scope()
        {
            if (table.Count > 1)
            {
                table.RemoveAt(table.Count - 1);
            }
        }

        /** Return the current scope depth, with 0 being the outer scope */
        public int scope_depth() { return table.Count - 1; }
    }
}
