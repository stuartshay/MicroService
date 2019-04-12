public static class Settings
{
    public static string ProjectName => "MicroService";

    public static string SonarUrl => "http://sonar.navigatorglass.com:9000";

    public static string SonarKey => "db762c49b56bd854f8e7fb1d03f7106468a27387";

    public static string SonarName => "MicroService";

    public static string SonarExclude => "/d:sonar.exclusions=Program.cs,**/Swagger/*.cs";

    public static string SonarExcludeDuplications => "/d:sonar.cpd.exclusions=**/GalleryContextExtensions.cs";

     public static string MyGetSource => "https://www.myget.org/F/microservice/api/v2/package";
}
