using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quotes_Server
{


    public class Quotes
    {
        private Random random = new Random();
        private List<string> myQuotes = new List<string>
        {
        "1. Красота нужна нам, чтобы нас любили мужчины, а глупость - чтобы мы любили мужчин. Коко Шанель",
        "2. Где любовь, там есть жизнь. М. Ганди",
        "3. В любви каждый становится поэтом. Платон",
        "4. Сколько лет люблю, а влюбляюсь в тебя каждый день. Лина Костенко",
        "5. В одном часу любви - целая жизнь. Оноре де Бальзак",
        "6. Любви всего мало. Она имеет счастье, а хочет рая, имеет рай - хочет неба. О любящие! Все это в вашей любви. Сумейте только найти. Виктор Мари Гюго"
        };

        public string GetQuote()
        {
            return myQuotes[random.Next(myQuotes.Count)];
        }



    }
}
