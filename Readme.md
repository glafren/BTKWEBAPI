# BTK Akademi ASP.NET Core Web API Projesi

Bu proje, BTK Akademi KPSAMında geliştirilen ASP.NET Core Web API projesini içermektedir. Proje, RESTful API prensiplerine uygun olarak tasarlanmış ve uygulanmıştır, aynı zamanda aşağıdaki özelliklere sahiptir:

## Özellikler

- RESTful API: Proje, HTTP metodlarını ve status kodlarını kullanarak kaynakları yönetir ve istemcilere uygun yanıtlar sunar.
- Filtreleme: API çağrılarına filtreler ekleyerek veri kümesini istenilen şekilde daraltabilirsiniz.
- Arama: Verileri belirli kriterlere göre arayarak istenen sonuçları elde edebilirsiniz.
- Doğrulama: Gelen verileri doğrulamak ve işlem öncesi hataları engellemek için doğrulama mekanizmaları kullanabilirsiniz.
- İçerik Pazarlığı: İstemcilere farklı veri formatlarında (JSON, XML vb.) yanıtlar sunabilirsiniz.
- Sıralama: Sonuçları belirli bir düzene göre sıralamak için sıralama parametreleri kullanabilirsiniz.
- Veri Şekillendirme: İstemci taleplerine göre veri şekillendirebilir ve ilişkili verileri içerecek şekilde yanıtlar oluşturabilirsiniz.
- HATEOAS: Hypermedia As The Engine Of Application State prensibine uygun olarak, yanıtlarınıza bağlantılar ekleyerek istemcilerin kolay gezinmesini sağlayabilirsiniz.
- Versiyonlama: API'nin farklı versiyonlarını yönetmek için versiyonlama stratejileri kullanabilirsiniz.
- Hız Sınırlama: API çağrılarını sınırlayarak aşırı yüklenmeyi önlemek için hız sınırlama mekanizmaları uygulayabilirsiniz.
- JWT (JSON Web Token): Kimlik doğrulama ve yetkilendirme için JWT kullanabilirsiniz.
- Postman: API'nizi test etmek ve belgelerini oluşturmak için Postman gibi araçlar kullanabilirsiniz.

## Başlangıç

Aşağıdaki adımları izleyerek projeyi yerel geliştirme ortamınızda çalıştırabilirsiniz:

1. Projeyi klonlayın veya indirin.
2. Visual Studio veya Visual Studio Code gibi bir IDE kullanarak projeyi açın.
3. Gerekli bağımlılıkları yüklemek için konsolu kullanarak `dotnet restore` komutunu çalıştırın.
4. `appsettings.json` dosyasında gerekli yapılandırmaları yapın, özellikle veritabanı bağlantı ayarları gibi.
5. Proje dizinindeyken terminalde `dotnet run` komutunu çalıştırarak projeyi başlatın.

## API Kullanımı

API'yi kullanarak çeşitli isteklerde bulunabilirsiniz. Örnek istekler ve yanıtlar için Postman koleksiyonunu inceleyebilirsiniz.
