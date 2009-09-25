using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using System.Windows.Forms;

using EnvDTE;
using EnvDTE80;
using Extensibility;
using Microsoft.VisualStudio.CommandBars;

using Laan.SQL.Formatter;

namespace Laan.AddIns.SqlFormatter
{
    public class Connect : IDTExtensibility2, IDTCommandTarget
    {
        private const string LaanSqlFormatter = "Laan.AddIns.SqlFormatter.Connect.LaanFormatSql";

        private Commands2 _commands;
        private DTE _application;
        private AddIn _addIn;

        public Connect()
        {
        }

        #region IDTExtensibility2 Members

        private bool CommandIsInstalled()
        {
            bool found = true;
            try
            {
                var findCommand = _commands.Item( LaanSqlFormatter, 1 );
            }
            catch
            {
                found = false;
            }
            return found;
        }

        private string GetToolsMenuName()
        {
            string toolsMenuName;

            try
            {
                //If you would like to move the command to a different menu, change the word "Tools" to the
                //  English version of the menu. This code will take the culture, append on the name of the menu
                //  then add the command to that menu. You can find a list of all the top-level menus in the file
                //  CommandBar.resx.
                string resourceName;
                ResourceManager resourceManager = new ResourceManager(
                    "Laan.AddIns.SqlFormatter.CommandBar",
                    Assembly.GetExecutingAssembly()
                );
                CultureInfo cultureInfo = new CultureInfo( _application.LocaleID );

                if ( cultureInfo.TwoLetterISOLanguageName == "zh" )
                {
                    System.Globalization.CultureInfo parentCultureInfo = cultureInfo.Parent;
                    resourceName = String.Concat( parentCultureInfo.Name, "Tools" );
                }
                else
                    resourceName = String.Concat( cultureInfo.TwoLetterISOLanguageName, "Tools" );

                toolsMenuName = resourceManager.GetString( resourceName );
            }
            catch
            {
                //We tried to find a localized version of the word Tools, but one was not found.
                //  Default to the en-US word, which may work for the current culture.
                toolsMenuName = "Tools";
            }
            return toolsMenuName;
        }

        private void PlaceCommandOnToolsMenu()
        {
            //Place the command on the tools menu.
            //Find the MenuBar command bar, which is the top-level command bar holding all the main menu items:
            var menuBarCommandBar = ( (Microsoft.VisualStudio.CommandBars.CommandBars) _application.CommandBars )[ "MenuBar" ];
            var toolsControl = menuBarCommandBar.Controls[ GetToolsMenuName() ];
            var toolsPopup = (CommandBarPopup) toolsControl;

            //This try/catch block can be duplicated if you wish to add multiple commands to be handled by your Add-in,
            //just make sure you also update the QueryStatus/Exec method to include the new command names.
            try
            {
                object[] contextGUIDS = new object[] { };

                Command command = _commands.AddNamedCommand2(
                    _addIn,
                    "LaanFormatSql",
                    "Format S&QL",
                    "Formats the current file",
                    true,
                    59,
                    ref contextGUIDS,
                    (int) vsCommandStatus.vsCommandStatusSupported + (int) vsCommandStatus.vsCommandStatusEnabled,
                    (int) vsCommandStyle.vsCommandStylePictAndText,
                    vsCommandControlType.vsCommandControlTypeButton
                );

                if ( command != null && toolsPopup != null )
                    command.AddControl( toolsPopup.CommandBar, 1 );

                command.Bindings = "Text Editor::Ctrl+`";

            }
            catch ( System.ArgumentException ex )
            {
                Trace.WriteLine( ex );
            }
        }

        public void OnConnection( object application, ext_ConnectMode connectMode, object addInInst, ref Array custom )
        {
            _addIn = (AddIn) addInInst;
            _application = ( (AddIn) addInInst ).DTE;
            _commands = (Commands2) _application.Commands;

            if ( !CommandIsInstalled() )
                PlaceCommandOnToolsMenu();
        }

        public void OnDisconnection( ext_DisconnectMode disconnectMode, ref Array custom )
        {
        }

        public void OnAddInsUpdate( ref Array custom )
        {
        }

        public void OnStartupComplete( ref Array custom )
        {
        }

        public void OnBeginShutdown( ref Array custom )
        {
        }

        #endregion

        #region IDTCommandTarget

        public void QueryStatus( string commandName, vsCommandStatusTextWanted neededText, ref vsCommandStatus status, ref object commandText )
        {
            if ( String.Compare( Path.GetExtension( _application.ActiveDocument.FullName ), ".sql", true ) != 0 )
                return;

            if ( neededText == vsCommandStatusTextWanted.vsCommandStatusTextWantedNone )
            {
                if ( commandName == LaanSqlFormatter )
                {
                    status = (vsCommandStatus) vsCommandStatus.vsCommandStatusSupported | vsCommandStatus.vsCommandStatusEnabled;
                    return;
                }
            }
        }

        public void Exec( string commandName, vsCommandExecOption executeOption, ref object varIn, ref object varOut, ref bool handled )
        {
            handled = false;
            if ( executeOption == vsCommandExecOption.vsCommandExecOptionDoDefault )
            {
                if ( commandName == LaanSqlFormatter )
                {
                    FormatSQL();
                    handled = true;
                    return;
                }
            }
        }

        #endregion

        private static int GetStartIndent( TextDocument textDocument )
        {
            var text = textDocument.Selection.Text;
            int index = 0;
            while ( ( text[ index ] == ' ' ) && ( text[ index ] != '\n' ) )
                index++;

            return index + textDocument.Selection.AnchorColumn;
        }

        private void FormatSQL()
        {
            TextDocument textDocument = (TextDocument) _application.ActiveDocument.Object( "TextDocument" );

            DateTime now = DateTime.Now;

            Cursor.Current = Cursors.WaitCursor;
            try
            {
                _application.StatusBar.Text = "Formatting SQL...";
                try
                {
                    _application.UndoContext.Open( "Format SQL", true );
                    try
                    {
                        if ( textDocument.Selection.IsEmpty )
                            textDocument.Selection.SelectAll();

                        var engine = new FormattingEngine();
                        engine.IndentStep = 0; // experimental: GetStartIndent( textDocument ) / engine.TabSize;

                        var output = engine.Execute( textDocument.Selection.Text );

                        // replace all text with formatted output
                        textDocument.Selection.Cut();
                        textDocument.Selection.Insert(
                            output,
                            (int) vsInsertFlags.vsInsertFlagsContainNewText
                        );
                    }
                    finally
                    {
                        textDocument.Selection.Cancel();
                        _application.UndoContext.Close();
                        _application.StatusBar.Text = String.Format( "SQL formatted in {0} seconds", ( DateTime.Now - now ).TotalSeconds );
                    }

                }
                catch ( Exception ex )
                {
                    Trace.WriteLine( ex );
                    _application.StatusBar.Text = "Error Formatting SQL: " + ex.Message;
                }
            }
            finally
            {
                Cursor.Current = Cursors.Default;
            }
        }
    }
}