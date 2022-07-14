using System;
using System.Runtime.Serialization.Formatters.Binary;
/*Stackte tutulan değerlerin boyutları biliniyor. Boyut bilindiği için Heap'e göre daha hızlıdır.
Heapte tutulan değerlerin boyutları bilinmez. Çalışma zamanında belirlenir. 
New tanımlanmış her değişkene başta bir değer atanır (0), null olmaz.
Heapte tutulan değerlere ulaşmak için; önce stackte o değişkenin bellek adresine erişilir sonra o bellek adresinin işaret ettiği heap kısmına gidip değişken değeri alınır.
Static tanımlamalara dikkat edilmelidir. Çünkü herkesin erişimine açıktır. Ortak kullanılan alanlarda static kullanılabilir. 
Mesela script key'e göre encryption ve decryption yapacaksak script key'i static yapabiliriz.
*/

// Bilinçsiz tür dönüşümü. Yani küçük türü büyük türe dönüştürdü ve bu dönüşümü açıkça bildirmedi.
byte a = 20;
int b;
b = a;

// float f = 20;
// double d;
// d=f;

// int a = 5;
// byte b = 3;
// short c = 30;
// double d =  a + b + c;

// a nın sayısal karşılığı gelecektir.
// Char c = 'a';
// decimal m;
// m = c;
// Console.WriteLine(m);


// Yemez. Sebebi de byteların toplamı byte ı aşar. 1 byte 8 bit e eşittir.
// byte a = 5;
// byte b = 10;
// byte c = a + b;

// Bilinçli tür dönüşümü.
// byte a = 5;
// byte b = 10;
// byte c = (byte)(a + b);

// sonuç 0 dır. Sebebi de; int tipi 4 byte yer kaplar ve 4 byte 32 bite eşittir. Byte ise 8 bit yer kaplar. 8 bit en fazla 255 değerini alabilir. 256 değeri 1 byte a sığmaz. 
// 2. byte değerine geçiş yapar 1. byte değeri de sıfırlanır.
// int a = 256;
// byte b ;
// b = (byte)a;
// Console.WriteLine(b);

// Kompleks (referans) bir tipten primitive tipe dönüştürmüş olduk: Boxing.
// Object c = 5;
// int a = (int)c;

// Primitive tipte olan bir değişkeni kompleks (referans) bir tipe dönüştürdük: Unboxing.
// int i = 50;
// Object o = (Object)i;

namespace memorymanagement{
        [Serializable]
        public class Address : IDisposable{
            private bool _disposed = false;
            public string Street {get; set;}
            public string City {get; set;}
            public string Country {get; set;}


            //Custom  Dispose
            //Tilde işaretiyle demek istediğimiz şöyle: Kapsam dışına çıkıldığında bu metodu çalıştır.
            ~Address() => Dispose(false);
             protected virtual void Dispose(bool disposing){
                if(_disposed){
                    return;
                }
                if(disposing){
                    //TODO
                    //Burada null yapılacak değişkenler varsa onlar halledilir.
                }
                _disposed = true;
            }

            public void Dispose(){
                Dispose(true);
                //Garbage collector çağırdık asıl iş burası. Belleği rahatlatacak olan kısım.
                GC.SuppressFinalize(this);
                
            }

        }
        [Serializable]
        public class Customer {
            public int Id { get; set;}
            public string FirstName { get; set;}
            public string LastName {get; set;}
            public Address Address { get; set; }
            
            
            public Customer ShallowCopy(){
                return this.MemberwiseClone() as Customer;
            }

            public Customer DeepCopy(){
                //Scoped: İşi olduğunda yeni bir nesne oluşturur ve  İŞİ BİTTİĞİNDE (kapsam dışına çıktığında) o nesneyi yok eder.
                //Tabi bunu yapmasını sağlayan IDisposable implementasyonudur. Peki bunu yapmamız ne işimize yarar?
                //40 bin kişinin aynı anda geldiğini düşünürsek bu durumda bellek iyice şişecektir ki burada bir yönetime ihtiyac duyacağız. Scoped imdadımıza yetişecektir.
                using(var ms = new MemoryStream()){
                    var formatter = new BinaryFormatter();
                    //Serialize işlemi birnevi atomlarına parçalama işlemidir.
                    formatter.Serialize(ms, this);
                    // bu kısım araştırılmalı.
                    ms.Position = 0;
                    // Tüm nesneyi yep yeni bir bellek adresinde tüm verileriyle klonladık.
                    return (Customer)formatter.Deserialize(ms);

                }
            }

        }
    internal class Program{
        static void Main(string[] args){
            
            /*!!!!IMPORTAND!!!!*/
           /*  Customer ali = new Customer(){Id = 1, FirstName ="Ali", LastName="Veli"};
            // İkiside aynı bellek adresini tutacağı için alinin FirstName ide artık veli olur. Veli Aliyi bozdu :)
            // Peki çözüm nedir? Veli abinin borularını ayırmaktır :)) --> ShallowCopy() metodu bu sorunun çözümüdür. Ne yapıyor bu ShallowCopy?
            // Bu iki nesnenin bellek adreslerini ayırıyor ama içerisindeki değişkenler yani tanımladığımız fieldlar aynı oluyor. Yani artık Veli Aliyi bozmuyor :)
            // Customer veli = ali;
            // aliyi aldık farklı bir bellek adresi oluşturup içerdiği değerleri veliye atadık. En başta Alinin değerleriyle aynı olur fakat sonradan
            // veli kendine göre özelleştirilebilir aliyi etkilemez.
            Customer veli = ali.ShallowCopy();
            veli.FirstName = "Veli";
            Console.WriteLine(ali.FirstName);
            Console.WriteLine(veli.FirstName); */


            /*!!!!IMPORTAND!!!!*/
            Customer ali = new Customer(){Id = 1, FirstName ="Ali", LastName="Veli", 
            Address = new Address() {City = "İstanbul", Street = "Feneryolu", Country = "Türkiye"}};
            // Tekrar geldik başa veli yine aliyi bozdu :)) Sebebi de ShallowCopy primitive tipleri kopyalar. Referans tipleri kopyalayamaz.
            // Çözümü ise DeepCopy() metodudur. Serialization, deserialization yapmamız gerekir. Bu iki işlemi ışınlanma olarka düşünebiliriz.
            // Serialization işlemi nesneyi atomlarına parçalayarak bir ışık hüzmesi haline getirir, deserialization ise yeni bir bellek adresi tanımlayıp bu ışık hüzmesini
            // o bellek adresinde yeniden oluşturur ve böylece ışınlanma gerçekleşmiş olur. Artık veli aliyi bozmuyor :))
            //Customer veli = ali.ShallowCopy();
            Customer veli = ali.DeepCopy();
            veli.Address.Street = "Kuzey";
            Console.WriteLine(ali.Address.Street);

            
            using(var ad = new Address()){

            }
            



            
        }
        
    }



}
