public static class Settings
{
    public static string ProjectName => "NavigatorAttractionsAPI";

    public static string SonarUrl => "http://sonar.navigatorglass.com:9000";

    public static string SonarKey => "***REMOVED-SONAR-KEY***";

    public static string SonarName => "NavigatorAttractionsAPI";

    public static string SonarExclude => "/d:sonar.exclusions=Program.cs,**/Swagger/*.cs";

    public static string SonarExcludeDuplications => "/d:sonar.cpd.exclusions=**/GalleryContextExtensions.cs";
}
