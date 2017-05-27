#if FULL_VERSION
// DslParser.cs - generated by the SLK parser generator 

namespace ScriptableData.Parser
{

    class DslParser
    {

        private static short[] Production = {0

,2,29,30 ,3,30,31,67 ,3,31,44,88 ,3,32,45,88 ,3,33,46,88
,3,34,47,88 ,3,35,48,88 ,3,36,49,88 ,3,37,50,88
,3,38,51,88 ,3,39,52,88 ,3,40,53,88 ,3,41,54,88
,3,42,55,88 ,3,43,56,88 ,3,44,45,68 ,3,45,46,69
,3,46,47,70 ,3,47,48,71 ,3,48,49,72 ,3,49,50,73
,3,50,51,74 ,3,51,52,75 ,3,52,53,76 ,3,53,54,77
,3,54,55,78 ,3,55,56,79 ,3,56,58,80 ,3,57,58,88
,3,58,93,59 ,2,59,81 ,4,59,94,61,95 ,5,60,65,96,82,83
,3,61,63,84 ,2,61,62 ,5,62,14,97,30,15 ,6,63,16,98,30,17,85
,6,63,18,100,30,19,86 ,4,63,20,64,87 ,8,64,101,93,94,65,102,95,88
,5,64,16,103,30,17 ,5,64,18,104,30,19 ,5,64,14,105,30,15
,3,65,21,89 ,3,65,22,106 ,3,65,23,107 ,3,65,24,108
,3,65,25,109 ,2,66,26 ,2,66,27 ,4,67,66,31,67 ,1,67
,6,68,1,89,90,32,68 ,1,68 ,10,69,2,89,91,33,2,89,92,33,69
,1,69 ,6,70,3,89,90,34,70 ,1,70 ,6,71,4,89,90,35,71
,1,71 ,6,72,5,89,90,36,72 ,1,72 ,6,73,6,89,90,37,73
,1,73 ,6,74,7,89,90,38,74 ,1,74 ,6,75,8,89,90,39,75
,1,75 ,6,76,9,89,90,40,76 ,1,76 ,6,77,10,89,90,41,77
,1,77 ,6,78,11,89,90,42,78 ,1,78 ,6,79,12,89,90,43,79
,1,79 ,6,80,13,89,90,57,80 ,1,80 ,5,81,94,60,95,81
,1,81 ,2,82,63 ,1,82 ,2,83,62 ,1,83 ,2,84,62 ,1,84
,3,85,99,63 ,1,85 ,3,86,99,63 ,1,86 ,3,87,99,63
,1,87
,0};

        private static int[] Production_row = {0

,1,4,8,12,16,20,24,28,32,36,40,44,48,52,56,60
,64,68,72,76,80,84,88,92,96,100,104,108,112,116,120,123
,128,134,138,141,147,154,161,166,175,181,187,193,197,201,205,209
,213,216,219,224,226,233,235,246,248,255,257,264,266,273,275,282
,284,291,293,300,302,309,311,318,320,327,329,336,338,345,347,353
,355,358,360,363,365,368,370,374,376,380,382,386
,0};

        private static short[] Parse = {

0,0,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2,2
,2,2,2,2,2,2,2,2,2,2,2,3,3,3,3,3,3,3,3,3
,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,3,4
,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4,4
,4,4,4,4,4,4,4,5,5,5,5,5,5,5,5,5,5,5,5,5
,5,5,5,5,5,5,5,5,5,5,5,5,5,5,5,6,6,6,6,6
,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6,6
,6,6,6,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7,7
,7,7,7,7,7,7,7,7,7,7,7,8,8,8,8,8,8,8,8,8
,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,8,9
,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9,9
,9,9,9,9,9,9,9,10,10,10,10,10,10,10,10,10,10,10,10,10
,10,10,10,10,10,10,10,10,10,10,10,10,10,10,10,11,11,11,11,11
,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11,11
,11,11,11,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12,12
,12,12,12,12,12,12,12,12,12,12,12,13,13,13,13,13,13,13,13,13
,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,13,14
,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14,14
,14,14,14,14,14,14,14,15,15,15,15,15,15,15,15,15,15,15,15,15
,15,15,15,15,15,15,15,15,15,15,15,15,15,15,15,16,16,16,16,16
,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16,16
,16,16,16,17,17,17,17,17,17,17,17,17,17,17,17,17,17,17,17,17
,17,17,17,17,17,17,17,17,17,17,17,18,18,18,18,18,18,18,18,18
,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,18,19
,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19,19
,19,19,19,19,19,19,19,20,20,20,20,20,20,20,20,20,20,20,20,20
,20,20,20,20,20,20,20,20,20,20,20,20,20,20,20,21,21,21,21,21
,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21,21
,21,21,21,22,22,22,22,22,22,22,22,22,22,22,22,22,22,22,22,22
,22,22,22,22,22,22,22,22,22,22,22,23,23,23,23,23,23,23,23,23
,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,23,24
,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24,24
,24,24,24,24,24,24,24,25,25,25,25,25,25,25,25,25,25,25,25,25
,25,25,25,25,25,25,25,25,25,25,25,25,25,25,25,26,26,26,26,26
,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26,26
,26,26,26,27,27,27,27,27,27,27,27,27,27,27,27,27,27,27,27,27
,27,27,27,27,27,27,27,27,27,27,27,28,28,28,28,28,28,28,28,28
,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,28,29
,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29,29
,29,29,29,29,29,29,29,30,30,30,30,30,30,30,30,30,30,30,30,30
,30,30,30,30,30,30,30,30,30,30,30,30,30,30,30,31,31,31,31,31
,31,31,31,31,31,31,31,31,32,31,32,31,32,31,32,31,31,31,31,31
,31,31,31,82,82,82,82,82,82,82,82,82,82,82,82,82,82,82,81,82
,81,82,81,82,82,82,82,82,82,82,82,88,88,88,88,88,88,88,88,88
,88,88,88,88,88,88,87,88,87,88,87,88,88,88,88,88,88,88,88,90
,90,90,90,90,90,90,90,90,90,90,90,90,90,90,89,90,89,90,89,90
,90,90,90,90,90,90,90,92,92,92,92,92,92,92,92,92,92,92,92,92
,92,92,91,92,91,92,91,92,92,92,92,92,92,92,92,1,1,1,1,1
,1,1,1,1,1,1,1,1,1,37,1,38,1,39,1,1,1,1,1,1
,1,1,1,84,84,84,84,84,84,84,84,84,84,84,84,84,83,84,36,84
,0,84,0,84,84,84,84,84,84,84,84,80,80,80,80,80,80,80,80,80
,80,80,80,80,35,80,34,80,34,80,34,79,79,79,79,79,80,80,80,86
,86,86,86,86,86,86,86,86,86,86,86,86,85,86,0,86,0,86,33,33
,33,33,33,0,86,86,86,78,78,78,78,78,78,78,78,78,78,78,78,77
,43,78,41,78,42,78,0,40,40,40,40,40,78,78,78,76,76,76,76,76
,76,76,76,76,76,76,75,49,50,76,0,76,0,76,44,45,46,47,48,0
,76,76,76,74,74,74,74,74,74,74,74,74,74,73,52,0,52,74,52,74
,0,74,0,0,0,51,51,52,74,74,74,72,72,72,72,72,72,72,72,72
,71,0,0,0,0,72,0,72,0,72,0,0,0,0,0,0,72,72,72,70
,70,70,70,70,70,70,70,69,0,0,0,0,0,70,0,70,0,70,0,0
,0,58,58,57,70,70,70,68,68,68,68,68,68,68,67,58,0,58,0,58
,0,68,0,68,0,68,58,58,58,0,0,0,68,68,68,66,66,66,66,66
,66,65,0,0,0,0,0,0,0,66,0,66,0,66,64,64,64,64,64,63
,66,66,66,0,0,0,0,0,64,0,64,0,64,62,62,62,62,61,0,64
,64,64,0,0,0,0,0,62,0,62,0,62,60,60,60,59,0,0,62,62
,62,0,0,0,0,0,60,0,60,0,60,56,55,0,53,0,0,60,60,60
,0,0,0,0,0,56,0,56,54,56,54,0,54,0,0,0,56,56,56,54
,54,54
};

        private static int[] Parse_row = {0

,953,1,29,57,85,113,141,169,197,225,253,281,309,337,365,393
,421,449,477,505,533,561,589,617,645,673,701,729,757,785,813,1036
,1009,983,952,1065,1092,1080,1118,1312,1309,1199,1290,1271,1252,1233,1205,1177
,1149,1121,1093,1065,1009,841,981,1037,869,897,925
,0};

        private static short[] Conflict = {

0
};

        private static int[] Conflict_row = {0


,0};

        private static short get_conditional_production(short symbol) { return (short)0; }

        private const short END_OF_SLK_INPUT_ = 28;
        private const short START_SYMBOL = 29;
        private const short START_STATE = 0;
        private const short START_CONFLICT = 93;
        private const short START_ACTION = 88;
        private const short END_ACTION = 110;
        private const short TOTAL_CONFLICTS = 0;

        internal const int NOT_A_SYMBOL = 0;
        internal const int NONTERMINAL_SYMBOL = 1;
        internal const int TERMINAL_SYMBOL = 2;
        internal const int ACTION_SYMBOL = 3;

        internal static int GetSymbolType(short symbol)
        {
            int symbol_type = NOT_A_SYMBOL;

            if (symbol >= START_ACTION && symbol < END_ACTION)
            {
                symbol_type = ACTION_SYMBOL;
            }
            else if (symbol >= START_SYMBOL)
            {
                symbol_type = NONTERMINAL_SYMBOL;
            }
            else if (symbol > 0)
            {
                symbol_type = TERMINAL_SYMBOL;
            }
            return symbol_type;
        }

        internal static bool IsNonterminal(short symbol)
        {
            return (symbol >= START_SYMBOL && symbol < START_ACTION);
        }

        internal static bool IsTerminal(short symbol)
        {
            return (symbol > 0 && symbol < START_SYMBOL);
        }

        internal static bool IsAction(short symbol)
        {
            return (symbol >= START_ACTION && symbol < END_ACTION);
        }

        internal static short GetTerminalIndex(short token)
        {
            return (token);
        }

        internal static short
        get_production(short conflict_number,
                         DslToken tokens)
        {
            short entry = 0;
            int index, level;

            if (conflict_number <= TOTAL_CONFLICTS)
            {
                entry = (short)(conflict_number + (START_CONFLICT - 1));
                level = 1;
                while (entry >= START_CONFLICT)
                {
                    index = Conflict_row[entry - (START_CONFLICT - 1)];
                    index += tokens.peek(level);
                    entry = Conflict[index];
                    ++level;
                }
            }

            return entry;
        }

        private static short
        get_predicted_entry(DslToken tokens,
                              short production_number,
                              short token,
                              int scan_level,
                              int depth)
        {
            return 0;
        }

        internal static void
        parse(DslAction action,
                DslToken tokens,
                DslError error,
                short start_symbol)
        {
            short lhs;
            short production_number, entry, symbol, token, new_token;
            int production_length, top, index, level;
            short[] stack = new short[512];

            top = 511;
            stack[top] = 0;
            if (start_symbol == 0)
            {
                start_symbol = START_SYMBOL;
            }
            if (top > 0)
            {
                stack[--top] = start_symbol;
            }
            else { error.message("DslParse: stack overflow\n"); return; }
            token = tokens.get();
            new_token = token;

            for (symbol = (stack[top] != 0 ? stack[top++] : (short)0); symbol != 0;)
            {

                if (symbol >= START_ACTION)
                {
                    action.execute(symbol - (START_ACTION - 1));

                }
                else if (symbol >= START_SYMBOL)
                {
                    entry = 0;
                    level = 1;
                    production_number = get_conditional_production(symbol);
                    if (production_number != 0)
                    {
                        entry = get_predicted_entry(tokens,
                                                      production_number, token,
                                                      level, 1);
                    }
                    if (entry == 0)
                    {
                        index = Parse_row[symbol - (START_SYMBOL - 1)];
                        index += token;
                        entry = Parse[index];
                    }
                    while (entry >= START_CONFLICT)
                    {
                        index = Conflict_row[entry - (START_CONFLICT - 1)];
                        index += tokens.peek(level);
                        entry = Conflict[index];
                        ++level;
                    }
                    if (entry != 0)
                    {
                        action.predict(entry);
                        index = Production_row[entry];
                        production_length = Production[index] - 1;
                        lhs = Production[++index];
                        if (lhs == symbol)
                        {
                            index += production_length;
                            for (; production_length-- > 0; --index)
                            {
                                if (top > 0)
                                {
                                    stack[--top] = Production[index];
                                }
                                else { error.message("DslParse: stack overflow\n"); return; }
                            }
                        }
                        else
                        {
                            new_token = error.no_entry(symbol, token, level - 1);
                        }
                    }
                    else
                    {                                       // no table entry
                        new_token = error.no_entry(symbol, token, level - 1);
                    }

                }
                else if (symbol > 0)
                {
                    if (symbol == token)
                    {
                        token = tokens.get();
                        new_token = token;
                    }
                    else
                    {
                        new_token = error.mismatch(symbol, token);
                    }

                }
                else
                {
                    error.message("\n parser error: symbol value 0\n");
                }

                if (token != new_token)
                {
                    if (new_token != 0)
                    {
                        token = new_token;
                    }
                    if (token != END_OF_SLK_INPUT_)
                    {
                        continue;
                    }
                }

                symbol = (stack[top] != 0 ? stack[top++] : (short)0);
            }

            if (token != END_OF_SLK_INPUT_)
            {
                error.input_left();
            }

        }

    };

}
#endif