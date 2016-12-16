using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace Huddled.Net
{
   public class TunableValidator
   {
      public static bool IgnoreChainErrors { get; set; }
      public static bool ShowConsoleStandardOutput { get; set; }
      private static readonly Dictionary<string, string> Trusted = new Dictionary<string, string>();
      private static bool _allowNextCert;
      private static bool _addNextCert;
      private static string _lasthash;
      private static string _lasthost;


      public static void WriteOutToConsole(string msg)
      {
          if (ShowConsoleStandardOutput)
          {
              Console.Out.Write("{1:yyy-MM-dd HH:mm:ss} {0}{2}", msg, DateTime.Now, Environment.NewLine);
          }
      }

      public static void SetValidator(bool ignoreChainErrors = false, Hashtable trustedCerts = null, bool showConsoleStandardOutput = true)
      {
         IgnoreChainErrors = ignoreChainErrors;
         ShowConsoleStandardOutput = showConsoleStandardOutput;

         if (trustedCerts != null)
         {
            foreach (var cert in trustedCerts.Keys)
            {
               Trusted[cert.ToString()] = trustedCerts[cert].ToString();
            }
         }
         ServicePointManager.ServerCertificateValidationCallback = TunableValidationCallback;
      }

      public static void ApproveLastRequest()
      {
         Trusted.Add(_lasthash, _lasthost);
         //         Console.WriteLine(string.Format("Added \"{0}\"=\"{1}\"", _lasthash, _lasthost));
         WriteOutToConsole("Added \"" + _lasthash + "\"=\"" + _lasthost + "\"");
      }

      public static void ApproveNextRequest(bool andTrustCert = false)
      {
         _allowNextCert = true;
         _addNextCert = andTrustCert;
      }



      public static Dictionary<string, string> TrustedCerts
      {
         get { return Trusted; }
      }

      private static bool TunableValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
      {
         bool result = true;
         var request = sender as WebRequest;

         // If there's no remote certificate, we're always going to fail (why are we even in here?)
         if (SslPolicyErrors.RemoteCertificateNotAvailable ==
             (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNotAvailable))
         {
            result = false;
         }
         else
         {
            // If the certificate doesn't match the address (like Splunk's self-signed cert)
            if (SslPolicyErrors.RemoteCertificateNameMismatch ==
                (sslPolicyErrors & SslPolicyErrors.RemoteCertificateNameMismatch))
            {
               result = TrustValidate(certificate, request);
            }

            // If the CA isn't trusted, and IgnoreChainErrors isn't set
            if (!IgnoreChainErrors && (SslPolicyErrors.RemoteCertificateChainErrors ==
                                       (sslPolicyErrors & SslPolicyErrors.RemoteCertificateChainErrors)))
            {
               result = TrustValidate(certificate, request);
            }
         }

         _lasthost = string.Empty;
         _lasthash = certificate.GetCertHashString();

         if (_addNextCert)
         {
            _addNextCert = false;
            ApproveLastRequest();
            result = true;
         }

         if (_allowNextCert)
         {
            _allowNextCert = false;
            result = true;
         }

         if (result) return true;


         Console.Error.WriteLine("Server SSL Certificate:");
         if (request != null)
         {
            _lasthost = request.RequestUri.Host;
            Console.Error.WriteLine("   Server: " + _lasthost);
         }
         Console.Error.WriteLine("  Subject: " + certificate.Subject);
         Console.Error.WriteLine("     Hash: " + _lasthash);
         Console.Error.WriteLine("Effective: " + certificate.GetEffectiveDateString());
         Console.Error.WriteLine("  Expires: " + certificate.GetExpirationDateString());
         if (request == null)
         {
            Console.Error.WriteLine(" ? Sender: " + sender);
         }


         Console.Error.WriteLine("   Errors: " + sslPolicyErrors);
         Console.Error.WriteLine(" Rejected: To accept, use Add-SessionTrustedCertificate to map the certificate and hostmask. Use the -LastFailed parameter to do it automatically.");

         return false;
      }

      private static bool TrustValidate(X509Certificate certificate, WebRequest request = null)
      {
         string host;
         return request != null &&
                Trusted.TryGetValue(certificate.GetCertHashString() ?? String.Empty, out host) &&
                request.RequestUri.Host == host;
      }
   }
}