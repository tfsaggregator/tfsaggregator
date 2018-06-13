.. title: Validating Self-Signed Certificates From .Net and PowerShell
.. slug: validating-self-signed-certificates-properly-from-powershell
.. date: 2014-07-28 01:30:03 UTC-04:00
.. tags: PowerShell, SSL, REST, WebRequest
.. link: 
.. description: A PowerShell module to allow weakening or circumventing SSL validation on web queries.
.. type: text

This module includes commands for importing certificates from files, loading them from the web server response of an http url, importing them to the Windows certificate store (to be trusted), and temporarily trusting them for a single PowerShell session.  It also includes proxy function command wrappers for ``Invoke-WebRequest`` and ``Invoke-RestMethod`` to add an ``-Insecure`` switch which allows single queries to ignore invalid SSL certificates.

**One caveat** is that the way that it works is based on implementing the ServerCertificateValidationCallback, and the *results of that callback are cached* by your system, so in testing I've found that if you allow a certain URL, that URL is going to be valid for several seconds until the cache expires, so if you make several repeated calls to the same URL, and only the first one is flagged ``-insecure``, they *may* all succeed. I can't remember what the documentation says about the timing or flushing the cache right now, but although it's obviously not a security issue, I am going to look for a way to flush that if it's possible.

Some Background
===============

The root of the problem is that the web request classes in .Net automatically validate SSL certificates, and until version 4.5 there was not an easy way to ignore SSL errors or skip the validation on an individual request. That means that when you try to get a web page from a server like the `computer science house at RIT`_, you get an error like::

   The underlying connection was closed: Could not establish trust relationship for the SSL/TLS secure channel.

For most linux command line apps like *curl* you have a parameter like ``--insecure`` which allows you to just ignore invalid SSL certificates for a single web request, but for .Net and PowerShell there is no such a flag, resulting in `a lot of questions on StackOverflow`_ and other such websites about the best way to deal with them.

Beginning with version 3.0, PowerShell ships with ``Invoke-WebRequest`` and ``Invoke-RestMethod`` cmdlets, but these cmdlets don't have an ``-insecure`` switch, and so you can't use them unless you disable validation.  The module I'm going to present here will fix that, and I want to show you how I did it so you can do the same thing for other similar cmdlets (like the ``Export-ODataEndpointProxy`` cmdlet that showed up in the July preview of PowerShell 5, and the cmdlets it generates).

Removing SSL Validation
=======================

There are a couple of common approaches to this problem in PowerShell (and .Net too, actually). The first is to just add the SSL certificate to your trusted store. The second is to set the ServicePointManager's ServerCertificateValidationCallback to a function that just returns TRUE -- thus ignoring **all** certificate errors.

That first solution is probably the right one, and I actually wrote a couple of function in this module to make it easier (one to fetch a cert from a web server, and another to add a certificate to your trusted store).  Of course, it would be even better to set up an actual trusted root certificate authority (for instance, using Active Directory), but the bottom line is that either of these require permanently altering the trust of the computer in order to make a web request. The problem is that if you're writing scripts, cmdlets or .Net applications that may be used by others, it basically amounts to telling your potential users they have to deal with the problem themselves (which, for the record, is exactly what Microsoft did when they wrote their web cmdlets for PowerShell 3).

So if you want to proactively avoid SSL errors, you have to set the ServerCertificateValidationCallback. For certain situations, it's enough to just do this in PowerShell::

   $url = "https://csh.rit.edu"
   $web = New-Object Net.WebClient
   [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true } 
   $output = $web.DownloadString($url)
   [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $null

Note, that I set the ServerCertificateValidationCallback back to ``$null`` when I was done -- this restores normal validation, so I have, in effect, only disabled the validation for that one call (since PowerShell isn't generally multi-threaded, I don't normally have to worry about other threads being affeected). However, setting the ServerCertificateValidationCallback to a scriptblock won't work for an asynchronous callback (one that happens on a task thread), because the other thread won't have a runspace to execute the script on.  So, if you try to use the same technique with Invoke-WebRequest or Invoke-RestMethod instead of calling the .Net ``WebClient`` methods directly, you'll just get a different error::

   The underlying connection was closed: An unexpected error occurred on a send.

If you dig in a little bit, you'll find that the InnerException of the error was::

   There is no Runspace available to run scripts in this thread. You can provide one in the DefaultRunspace property
   of the System.Management.Automation.Runspaces.Runspace type. The script block you attempted to invoke was:  $true

Obviously this approach doesn't work at that point, and the Invoke-WebRequest cmdlet is not the only place which will call web APIs on background threads, so to make those cmdlets and APIs work, we need to write it in C# (and do so in a way that's flexible enough that we can control it from PowerShell). 

Additionally, simply returning true will disable all validation, and that's not really a safe practice -- it's certainly not what you want to do all of the time. If you're on .Net 4.5 or later, you can set the callback on the a raw HttpWebRequest and only affect that one request, but obviously that only works if you write your code at that low level, don't use the PowerShell cmdlets, and are on the latest version of .Net -- plus, you still have to write the logic that determines when that should happen.  

The bottom line is that what we probably want is something like an ``insecure`` switch on the PowerShell cmdlets, so that we can make a specific request be insecure, and then when we were writing functions, we could just pass that parameter up to our functions.


Custom SSL Validator
====================

There are several example validators on the `Using Trusted Roots Respectfully`_ page from mono project, but I'm going to try to blend a few of those validators to give us the option of tuning the SSL validation to our specific needs.

The point is that I don't want to weaken *all* validation, I just want to trust a specific cert for a specific domain, or perhaps just ignore problems on one domain, or make one specific request regardless of whether the SSL certificate is valid or not.

Let's look at the callback and see the information we have to work with::

   bool TunableValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)

#. The sender is usually going to be the WebRequest that was calling an https domain that failed validation.  
#. The certificate, of course, is the one the actually failed ...
#. The chain is the series of certificates that issued the original one, back to the root certificate authority, along with the trust information about them.
#. The sslPolicyErrors tells us what went wrong: Was there no cert? Was the cert for the wrong domain? Was the root CA not trusted?

So, what I've written is first a check for the three main SSL errors, and a way to pre-emptively ignore them once, or post-humously trust a certificate that failed the first time, as well as some better error messages (which have to be output using Console.Error.WriteLine rather than Write-Error because they might be running on a background thread).

.. 
    This should turn into something like a cucumber spec...
..
    #. I want to be sure I'm not weakening validation for requests that I don't mean to affect.
    #. I want to be able to just trust a few specific certificate(s).
    #. I want to be able to just ignore problems for a single web request.
       except the ones that I specifically override security on.

.. _a lot of questions on StackOverflow: http://stackoverflow.com/search?q=self-signed+SSL+certificates+[csharp]+OR+[powershell]
.. _Using Trusted Roots Respectfully: http://www.mono-project.com/UsingTrustedRootsRespectfully
.. _HttpWebRequest: http://msdn.microsoft.com/en-us/library/system.net.httpwebrequest.servercertificatevalidationcallback.aspx
.. _computer science house at RIT: https://csh.rit.edu
