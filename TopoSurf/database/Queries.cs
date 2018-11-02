

namespace SqlTest
{
    public static class Queries
    {
        #region matieres     
        public static string selectMatiere = "SELECT * FROM matiere;";
        public static string insertMatiere = "INSERT INTO matiere(type , meas) VALUES ('";
        public static string boughtUnit = "SELECT * FROM matierePointAchat WHERE matierePointAchat.matiereId = ";
        #endregion

        #region points d'achat
        public static string selectPA = "SELECT * FROM pointAchat;";
        public static string totalPayment = "SELECT * FROM matierePointAchat WHERE matierePointAchat.pointAchatId = ";

        #endregion
    }
}
