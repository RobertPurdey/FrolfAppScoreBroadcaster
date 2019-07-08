using System;
using System.Security.Cryptography;

namespace FrolfAppScoreBroadcaster._04._Encryption
{
    public class RsaKeyInfo
    {
        private static string Modulus   = "9zZBzXhq2GE2iDhwrjtI4goUARU2d2/R0TGZPiesbn6wOI7uNPePEhd3kaev8sa0Kb79S6oJdrD/0uf7FjUgEB6qtfC/gK3q0HofEFMzyAKyoJSqxWMd3s4bdBFYu9cWttHhNiwK0WbYjJMmUUUkRjkIVhPx6M6cQbKz45bEfl+OZUpC/JMMlUuIgQ4gqecKqdeV+de3Pk+hdTu5YgS0fPAu3WxiNBbFJ9l1rCyNkYDKdIH8GWGXon3Y70MT0P1vai+/VoKx9L2X3L2Dhl31zf1LTK7l1N/WIBYld/InA7ZtqLb/xJdWiNnoMUwFcoNCSCr3PEeMm9gk+g4bbWJfjQ==";
        private static string Exponent  = "AQAB";
        private static string P         = "/5SbCiQgX4RyPZmtbETZmvRPxw1Kw1QM9/OtZb2fVqhSALz56v6rwlqGyXi0dk2wJQFw84BNfHqhaSPdyPxwjZaJXnBuFyI+wtkigCJBJND/L7ner+BjKO4J9v3agMxtv5J6pgAiU4803ihA11QBmQh9uEE7dO5gJxiLswP6fJc=";
        private static string Q         = "954ijX4Rdd9vd9GPDZOysDR+TTdAC6B2hfpogsoQLhxSP1cnzNxmA/xE1Hde0k8EFhjudf6aGGnCskeYaeneuDskhNjXnf97ZES7cFo6o/JXS1xGVbD9fmXWyrjVqizW6fOkDZzLYU5B+sjrXUMDrn1VGPJ2K1/pV99Td0kU9Xs=";
        private static string DP        = "RNvL3bKYCkQL527VG5t9KVNzfwSkxPWLPO6pJAUvvdBBr7M6fka5DfcH45YiwNDziTTXMrO5rLT5cfNY2MKyrGMHhasy7gaq9CI+OlmARaQNbNVeGvKQpMFla+c/DH6Hfxq+8qSMmwi1TLl5psoaWBnCjXb9xuZGf6IMWWHUBec=";
        private static string DQ        = "czUkJmjtfsZCeqEJyetaTBlgWlTGe6JhAt0LGy8gcBPFQKswXWR+IoSREbmoaHlTEWTwLf4TfCBY8dHV3BFwCo+Z4iVxzJU9t90yyIdymSz76Jg6MUxz5QdE9HUjFFZgd+FgBuVYyyE6GZC50V6Iq/qsSTsmN/AcBUJm9y0Nj1k=";
        private static string InverseQ  = "rcVwRlpUIJtu1148IHE7R/HqgJOJrYZ0JZez85a8OrKhpr+pZdCnlaci7hellMDpDY0QelQgW+3NR6jzuQwhAbfw5s7x1nzskpmS8/2zSkFnVK9/1bXvhWupqloR+25VteTMWQ9UgyAU1mdFByEO9X9sXiOuLkUQddXe9gMTXRA=";
        private static string D         = "zoPI3LjnqPMs9wcPOr3T2ODKbU0nPwduo+9nMQE7juLOm7DrVdwo7NglzsvitFFCWE1wlDDrzvd1/t5EZvziWBUGTw9bK0gejSI3qQ+YhlGan4MSVerDHUnYrVGAawr3sqoKFZMdRmlAJc8Xh3TXJMKoMCBhSjavWkLK/CkK5PWSRWw2CT6FOQZKjEOjs+i3ajhi3u3sbxnV5CS3IG1he/gS9p0lSVBe/o0t17jUXhw09PtEtT+P4vbl1qDkk6gCn6B8D6MPIcgR+6IR14PWVLud3LG5YylwEq/XMa7Jun/ndW78z3Vp1R7fspXhiDrprEivHSUBTpkIHXLRhEtCkQ==";

        public static RSAParameters GetRsaParameters()
        {
            return new RSAParameters()
            { 
                Modulus  = Convert.FromBase64String(Modulus),
                Exponent = Convert.FromBase64String(Exponent),
                P        = Convert.FromBase64String(P),
                Q        = Convert.FromBase64String(Q),
                DP       = Convert.FromBase64String(DP),
                DQ       = Convert.FromBase64String(DQ),
                InverseQ = Convert.FromBase64String(InverseQ),
                D        = Convert.FromBase64String(D)
            };
        }

        public static string GetPrivateKeyXml()
        {
            return  "<RSAKeyValue>"
                +       "<Modulus>9zZBzXhq2GE2iDhwrjtI4goUARU2d2/R0TGZPiesbn6wOI7uNPePEhd3kaev8sa0Kb79S6oJdrD/0uf7FjUgEB6qtfC/gK3q0HofEFMzyAKyoJSqxWMd3s4bdBFYu9cWttHhNiwK0WbYjJMmUUUkRjkIVhPx6M6cQbKz45bEfl+OZUpC/JMMlUuIgQ4gqecKqdeV+de3Pk+hdTu5YgS0fPAu3WxiNBbFJ9l1rCyNkYDKdIH8GWGXon3Y70MT0P1vai+/VoKx9L2X3L2Dhl31zf1LTK7l1N/WIBYld/InA7ZtqLb/xJdWiNnoMUwFcoNCSCr3PEeMm9gk+g4bbWJfjQ==</Modulus>"
                +       "<Exponent>AQAB</Exponent>"
                +       "<P>/5SbCiQgX4RyPZmtbETZmvRPxw1Kw1QM9/OtZb2fVqhSALz56v6rwlqGyXi0dk2wJQFw84BNfHqhaSPdyPxwjZaJXnBuFyI+wtkigCJBJND/L7ner+BjKO4J9v3agMxtv5J6pgAiU4803ihA11QBmQh9uEE7dO5gJxiLswP6fJc=</P>"
                +       "<Q>954ijX4Rdd9vd9GPDZOysDR+TTdAC6B2hfpogsoQLhxSP1cnzNxmA/xE1Hde0k8EFhjudf6aGGnCskeYaeneuDskhNjXnf97ZES7cFo6o/JXS1xGVbD9fmXWyrjVqizW6fOkDZzLYU5B+sjrXUMDrn1VGPJ2K1/pV99Td0kU9Xs=</Q>"
                +       "<DP>RNvL3bKYCkQL527VG5t9KVNzfwSkxPWLPO6pJAUvvdBBr7M6fka5DfcH45YiwNDziTTXMrO5rLT5cfNY2MKyrGMHhasy7gaq9CI+OlmARaQNbNVeGvKQpMFla+c/DH6Hfxq+8qSMmwi1TLl5psoaWBnCjXb9xuZGf6IMWWHUBec=</DP>"
                +       "<DQ>czUkJmjtfsZCeqEJyetaTBlgWlTGe6JhAt0LGy8gcBPFQKswXWR+IoSREbmoaHlTEWTwLf4TfCBY8dHV3BFwCo+Z4iVxzJU9t90yyIdymSz76Jg6MUxz5QdE9HUjFFZgd+FgBuVYyyE6GZC50V6Iq/qsSTsmN/AcBUJm9y0Nj1k=</DQ>"
                +       "<InverseQ>rcVwRlpUIJtu1148IHE7R/HqgJOJrYZ0JZez85a8OrKhpr+pZdCnlaci7hellMDpDY0QelQgW+3NR6jzuQwhAbfw5s7x1nzskpmS8/2zSkFnVK9/1bXvhWupqloR+25VteTMWQ9UgyAU1mdFByEO9X9sXiOuLkUQddXe9gMTXRA=</InverseQ>"
                +       "<D>zoPI3LjnqPMs9wcPOr3T2ODKbU0nPwduo+9nMQE7juLOm7DrVdwo7NglzsvitFFCWE1wlDDrzvd1/t5EZvziWBUGTw9bK0gejSI3qQ+YhlGan4MSVerDHUnYrVGAawr3sqoKFZMdRmlAJc8Xh3TXJMKoMCBhSjavWkLK/CkK5PWSRWw2CT6FOQZKjEOjs+i3ajhi3u3sbxnV5CS3IG1he/gS9p0lSVBe/o0t17jUXhw09PtEtT+P4vbl1qDkk6gCn6B8D6MPIcgR+6IR14PWVLud3LG5YylwEq/XMa7Jun/ndW78z3Vp1R7fspXhiDrprEivHSUBTpkIHXLRhEtCkQ==</D>"
                +   "</RSAKeyValue>";
        }

        public static string GetPublicKeyXml()
        {
            return "<RSAKeyValue>"
                +      "<Modulus>9zZBzXhq2GE2iDhwrjtI4goUARU2d2/R0TGZPiesbn6wOI7uNPePEhd3kaev8sa0Kb79S6oJdrD/0uf7FjUgEB6qtfC/gK3q0HofEFMzyAKyoJSqxWMd3s4bdBFYu9cWttHhNiwK0WbYjJMmUUUkRjkIVhPx6M6cQbKz45bEfl+OZUpC/JMMlUuIgQ4gqecKqdeV+de3Pk+hdTu5YgS0fPAu3WxiNBbFJ9l1rCyNkYDKdIH8GWGXon3Y70MT0P1vai+/VoKx9L2X3L2Dhl31zf1LTK7l1N/WIBYld/InA7ZtqLb/xJdWiNnoMUwFcoNCSCr3PEeMm9gk+g4bbWJfjQ==</Modulus>"
                +      "<Exponent>AQAB</Exponent>"
                +  "</RSAKeyValue>";
        }
    }
}
