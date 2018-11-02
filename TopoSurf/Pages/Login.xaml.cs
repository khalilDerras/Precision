using System.Windows;
using System.Windows.Controls;
using System.Data;
using TopoSurf.MessageBoxStyle;

namespace TopoSurf.Pages
{

    public partial class Login : Window
    {
        Label l;
        Button butt1;
        Menu butt2;
        public static string currentUser = "";
        public Login()
        {
            InitializeComponent();
        }
        public Login(Label label, Button button1, Menu button2)
        {
            butt1 = button1;
            butt2 = button2;
            l = label;
            InitializeComponent();
        }
        public static void Logout()
        {
            currentUser = "";
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void sign_in_Click(object sender, RoutedEventArgs e)
        {
            sign_up.IsChecked = false;
        }

        private void sign_up_Click(object sender, RoutedEventArgs e)
        {
            sign_in.IsChecked = false;
        }

        static Database db = new Database();

        private void login_Click(object sender, RoutedEventArgs e)
        {

            var id = "'" + user2.Text + "'";
            var pass = "'" + pass2.Password + "'";
            db.commande.CommandText = "CREATE TABLE `users` ( `id` INTEGER NOT NULL UNIQUE, `username` TEXT NOT NULL UNIQUE, `password` TEXT NOT NULL UNIQUE, `email` TEXT NOT NULL UNIQUE, PRIMARY KEY(`id`) )";
            try { db.commande.ExecuteNonQuery(); }
            catch (System.Exception) { }

            db.commande.CommandText = "SELECT * FROM users WHERE users.username = " + id + "and users.password= " + pass;

            try
            {
                db.dataReader = db.commande.ExecuteReader();
                db.dataReader.Read();
                currentUser = db.dataReader.GetString(1);
                (new MssgBox("logged in as : " + currentUser)).ShowDialog();
                l.Content = user2.Text;
                butt1.Width = 0;
                butt2.Width = 200;
                butt2.HorizontalAlignment = HorizontalAlignment.Right;
                db.dataReader.Close();
                this.Close();

            }
            catch (System.Exception)
            {
                db.dataReader.Close();
                (new MssgBox("Wrong username or password !")).ShowDialog();

            }

            /**/
            // MessageBox.Show("id : " + db.dataReader.GetInt32(0).ToString() + "nom : " + db.dataReader.GetString(1) + "MDP : " + db.dataReader.GetString(2));
        }

        private void signup_Click(object sender, RoutedEventArgs e)
        {
            var user = username.Text;
            var pass = password.Password;
            var eml = email.Text;
            db.commande.CommandText = "CREATE TABLE `users` ( `id` INTEGER NOT NULL UNIQUE, `username` TEXT NOT NULL UNIQUE, `password` TEXT NOT NULL UNIQUE, `email` TEXT NOT NULL UNIQUE, PRIMARY KEY(`id`) )";
            try { db.commande.ExecuteNonQuery(); }
            catch (System.Exception) { }
            //MessageBox.Show(password.Password);
            db.commande.CommandText = "INSERT INTO users (username , password ,email ) VALUES ('" + user + "','" + pass + "','" + eml + "'); ";

            try
            {
                db.commande.ExecuteNonQuery();
                l.Content = username.Text;
                butt1.Width = 0;
                butt2.Width = 200;
                butt2.HorizontalAlignment = HorizontalAlignment.Right;
                currentUser = username.Text;
                (new MssgBox("Welcome " + currentUser)).ShowDialog();
                this.Close();
            }
            catch (System.Exception)
            {
                (new MssgBox("Already used username or E-mail !")).ShowDialog();
            }
        }
    }
}
