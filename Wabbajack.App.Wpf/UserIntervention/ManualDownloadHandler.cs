using System;
using System.Threading;
using System.Threading.Tasks;
using Wabbajack.DTOs.DownloadStates;
using Wabbajack.DTOs.Interventions;

namespace Wabbajack;

public class ManualDownloadHandler : BrowserWindowViewModel
{
    public ManualDownload Intervention { get; set; }

    public ManualDownloadHandler(IServiceProvider serviceProvider) : base(serviceProvider) { }

    protected override async Task Run(CancellationToken token)
    {
        var uri = default(ManualDownload.BrowserDownloadState);
        try
        {
            var archive = Intervention.Archive;
            var md = Intervention.Archive.State as Manual;

            HeaderText = $"Manual download for {archive.Name} ({md.Url.Host})";

            Instructions = string.IsNullOrWhiteSpace(md.Prompt) ? $"Please download {archive.Name}" : md.Prompt;

            var task = WaitForDownloadUri(token, async () =>
            {
                await RunJavaScript("Array.from(document.getElementsByTagName(\"iframe\")).forEach(f => {if (f.title != \"SP Consent Message\" && !f.src.includes(\"challenges.cloudflare.com\")) f.remove()})");
            });
            await NavigateTo(md.Url);

            await RunJavaScript(@"
                (function() {
                    function findAndClickButton() {
                        // Try finding by ID first
                        var slowButton = document.getElementById('slowDownloadButton');
                        
                        // If not found by ID, try finding by text content
                        if (!slowButton) {
                            var buttons = document.querySelectorAll('button');
                            for (var i = 0; i < buttons.length; i++) {
                                if (buttons[i].innerText.trim().toLowerCase() === 'slow download' || 
                                    buttons[i].textContent.trim().toLowerCase() === 'slow download' ||
                                    buttons[i].querySelector('span') && buttons[i].querySelector('span').textContent.trim().toLowerCase() === 'slow download') {
                                    slowButton = buttons[i];
                                    console.log('Found slow download button by text');
                                    break;
                                }
                            }
                        }
                        
                        if (!slowButton) {
                            console.log('Slow download button not found, retrying in 500ms');
                            setTimeout(findAndClickButton, 500);
                            return;
                        }
                        
                        // First make sure the button is visible
                        slowButton.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        
                        // Force center the button in the viewport using calculated positions
                        var rect = slowButton.getBoundingClientRect();
                        var buttonMiddle = rect.top + rect.height / 2;
                        var viewportHeight = window.innerHeight;
                        var scrollY = window.scrollY + buttonMiddle - viewportHeight / 2;
                        window.scrollTo({ top: scrollY, behavior: 'smooth' });
                        console.log('Centering button at position:', scrollY);
                        
                        // Wait a moment for the scroll to complete
                        setTimeout(function() {
                            // Click the button
                            console.log('Clicking slow download button');
                            slowButton.click();
                            
                            // Close the window after clicking
                            setTimeout(function() {
                                window.close();
                            }, 2000);
                        }, 1000);
                    }
                    
                    // Start the process
                    findAndClickButton();
                })();");
                
            uri = await task;
        }
        finally
        {
            Intervention.Finish(uri);
        }
    }
}