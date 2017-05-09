using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Npgsql.EntityFrameworkCore.PostgreSQL.FunctionalTests.Utilities
{
    public class ConnectionCreator
    {
        public static NpgsqlConnection CreateConnection(string databaseName)
        {
            if ( true )
            {
                var builder = new NpgsqlConnectionStringBuilder
                {
                    Host = "jakenetwork.org",
                    Port = 26257,
                    Username = "root",
                    Password = "***",
                    Database = databaseName,
                    SslMode = SslMode.Require,
                    UseSslStream = true,
                    ServerCompatibilityMode = ServerCompatibilityMode.Redshift
                };
                var connection = new NpgsqlConnection( builder.ToString() );
                connection.ProvideClientCertificatesCallback += GetClientCertificates;
                connection.UserCertificateValidationCallback += ( sender, certificate, chain, errors ) => true;
                return connection;
            }
            else
            {
                var connectionString = new NpgsqlConnectionStringBuilder( TestEnvironment.DefaultConnection )
                {
                    Database = databaseName
                }.ConnectionString;
                return new NpgsqlConnection( connectionString );
            }
        }


        private static void GetClientCertificates( X509CertificateCollection certificates )
        {
            certificates.Add( new X509Certificate( "C:\\Users\\Jake\\Downloads\\CockroachDBTest\\certs\\ca.cert" ) );
            certificates.Add( new X509Certificate2( "C:\\Users\\Jake\\Downloads\\CockroachDBTest\\certs\\cockroachdb-01.pfx", "Freddie143?", X509KeyStorageFlags.MachineKeySet ) );
        }

        public static NpgsqlConnection CreateConnection()
        {
            return new NpgsqlConnection( TestEnvironment.DefaultConnection );
        }
    }
}
