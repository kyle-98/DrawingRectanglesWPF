using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Diagnostics; //Only used for debugging (allows printing to the console).
using System.Windows.Forms;

namespace Drawing_Rectangles
{
     /// <summary>
     /// Interaction logic for MainWindow.xaml
     /// </summary>
     public partial class MainWindow : Window
     {
          public MainWindow()
          {
               InitializeComponent();
          }

          //Function triggered on user clicking "Open File" button and opens a file dialog to only allow the selection of images from png, jpg, and jpeg source formatting.
          private void Open_file_btn_Click(object sender, RoutedEventArgs e)
          {
               Microsoft.Win32.OpenFileDialog open_file = new Microsoft.Win32.OpenFileDialog();
               open_file.Title = "Open an image";
               open_file.Filter = "All supported graphics|*.jpg;*.jpeg;*.png";

               if (open_file.ShowDialog() == true)
               {
                    custom_image.Source = new BitmapImage(new Uri(open_file.FileName));
                    custom_image.Width = custom_image.Source.Width;
                    custom_image.Height = custom_image.Source.Height;
               }

          }

          private bool is_mouse_down = false;
          private bool is_editing = false;
          private bool is_moving = false;
          private Rectangle custom_rect;
          private Rectangle selected_rect;
          private Point drawing_starting_point;
          private Point editing_starting_point;
          private Point moving_starting_point;
          private Color rect_color = Colors.Red;

          //Function triggered when user clicks anywhere on the canvas.
          private void Canvas_mouse_down(object sender, MouseButtonEventArgs e)
          {
               is_mouse_down = (e.ButtonState == MouseButtonState.Pressed) && (e.ChangedButton == MouseButton.Left);

               if (!is_mouse_down) { return; }

               //Check if the user double clicks specifically on a previously created rectangle.  This enables movement of the old rectangle object.
               if (e.ClickCount == 2 && e.OriginalSource is Rectangle)
               {
                    selected_rect = (Rectangle)e.OriginalSource;
                    is_moving = true;
                    is_editing = false;

                    moving_starting_point = e.MouseDevice.GetPosition(main_canvas);
               }

               //Check if the user triple clicks on a rectangle.  If so, this enable the color changing aspect of the rectangle objects.
               else if(e.ClickCount == 3 && e.OriginalSource is Rectangle)
               {
                    selected_rect = (Rectangle)e.OriginalSource;
                    selected_rect.Fill = new SolidColorBrush(rect_color);
               }

               //Handles other actions done for rectangles objects.
               else
               {
                    is_moving = false;
                    
                    //Checks if the user has clicked on a previously created retangle object and if so allows for editing the size of it.
                    if (e.OriginalSource is Rectangle)
                    {
                         selected_rect = (Rectangle)e.OriginalSource;
                         is_editing = true;
                         editing_starting_point = e.MouseDevice.GetPosition(main_canvas);
                    }

                    //If the user has only clicked on blank canvas area of an image then this allows for the creation of a new rectangle object.
                    else
                    {
                         is_editing = false;
                         custom_rect = new Rectangle();
                         drawing_starting_point = e.MouseDevice.GetPosition(main_canvas);
                         custom_rect.Fill = new SolidColorBrush(rect_color);
                         main_canvas.Children.Add(custom_rect);
                    }
               }


          }

          //Function handles updating and tracking the user's mouse movement over the canvas.
          private void Canvas_mouse_move(object sender, System.Windows.Input.MouseEventArgs e)
          {
               if (!is_mouse_down) { return; }

               //If the user is in edit mode of a rectangle, then the rectangle will have full control as if it is being created for the first time.
               //This calculates the rectangles heigh and width based on the previous position of the top corner and where the mouse has been dragged to.
               if (is_editing)
               {
                    Point curr_pos_editing = e.MouseDevice.GetPosition(main_canvas);

                    selected_rect.SetValue(Canvas.LeftProperty, Math.Min(curr_pos_editing.X, editing_starting_point.X));
                    selected_rect.SetValue(Canvas.TopProperty, Math.Min(curr_pos_editing.Y, editing_starting_point.Y));

                    selected_rect.Width = Math.Abs(curr_pos_editing.X - editing_starting_point.X);
                    selected_rect.Height = Math.Abs(curr_pos_editing.Y - editing_starting_point.Y);
               }

               //If the user is in movement mode then the rectangle object's coordinates change based on the top left corner of the rectangle in respect to where the user is moving their mouse.
               //This also prevents the user from dragging the rectangles outside of the canvas object.  The other portion of that is handled in MainWindow.xaml where the option ClipToBounds="True" is set to true for the canvas layer.
               else if (is_moving)
               {
                    Point curr_pos_moving = e.MouseDevice.GetPosition(main_canvas);

                    if (Canvas.GetRight(selected_rect) > custom_image.Width) { Canvas.SetRight(selected_rect, custom_image.Width - selected_rect.Width); }

                    selected_rect.ClipToBounds = true;
                    selected_rect.SetValue(Canvas.LeftProperty, curr_pos_moving.X);
                    selected_rect.SetValue(Canvas.RightProperty, curr_pos_moving.X + selected_rect.Width);
                    selected_rect.SetValue(Canvas.BottomProperty, curr_pos_moving.Y - selected_rect.Height);
                    selected_rect.SetValue(Canvas.TopProperty, curr_pos_moving.Y);


               }

               //If the user is creating a new rectangle the height and width of the rectangle is calculated based on the starting point of the left click and the ending point of the release.
               else
               {
                    Point curr_pos = e.MouseDevice.GetPosition(main_canvas);

                    custom_rect.SetValue(Canvas.LeftProperty, Math.Min(curr_pos.X, drawing_starting_point.X));
                    custom_rect.SetValue(Canvas.TopProperty, Math.Min(curr_pos.Y, drawing_starting_point.Y));

                    custom_rect.Width = Math.Abs(curr_pos.X - drawing_starting_point.X);
                    custom_rect.Height = Math.Abs(curr_pos.Y - drawing_starting_point.Y);
               }


          }

          //Function that handles when the user has released the left mouse button on the canvas.
          //This updates the variable named "is_mouse_down" which lets the rest of the code to know to stop editing, moving, or creating because the user has let go of the button.
          private void Canvas_mouse_up(object sender, MouseButtonEventArgs e)
          {
               if (e.ChangedButton == MouseButton.Left) { is_mouse_down = false; }
          }

          //Function that is trigged upon a right click event if the user is right clicking on a previously created rectangle object.
          //This function then removes the selected rectangle from the canvas permanently.
          private void Delete_selected_rect(object sender, MouseButtonEventArgs e)
          {

               if (e.OriginalSource is Rectangle)
               {
                    selected_rect = (Rectangle)e.OriginalSource;

                    main_canvas.Children.Remove(selected_rect);

               }
          }

          //This function handles saving the canvas as a new image.
          //This is triggered upon clicking the "save image" button in which prompts a dialog to save the to wherever the user pleases.
          //This uses a Png encoder to translate the canvas to a savable png file.
          private void Save_image(object sender, RoutedEventArgs e)
          {
               Microsoft.Win32.SaveFileDialog save_file = new Microsoft.Win32.SaveFileDialog();
               save_file.Title = "Save your image";
               save_file.Filter = "All supported graphics|*.jpg;*.jpeg;*.png";
               if (save_file.ShowDialog() == true)
               {
                    int image_width = Convert.ToInt32(custom_image.Source.Width);
                    int image_height = Convert.ToInt32(custom_image.Source.Height);

                    RenderTargetBitmap render = new RenderTargetBitmap((int)main_canvas.Width, (int)main_canvas.Height, 100.0, 100.0, PixelFormats.Pbgra32);

                    PngBitmapEncoder encoder = new PngBitmapEncoder();
                    render.Render(main_canvas);
                    encoder.Frames.Add(BitmapFrame.Create(render));
                    using (var stream = save_file.OpenFile())
                    {
                         encoder.Save(stream);
                    }
               }
          }

          //This function handles choosing custom colors for the rectangles.
          //This is triggered when the user clicks the "color picker" button and then prompts a color picking dialog for the user to choose or create a custom color in.
          //This also converts the Color object from a System.Drawing.Color object to a System.Windows.Media.Color object in which the rectangle is able to parse.
          //This updates a variable and is used to change the color of newly created rectangles as well as updating old ones.
          private void color_btn_Click(object sender, RoutedEventArgs e)
          {
               ColorDialog color_picker = new ColorDialog();
               color_picker.AllowFullOpen = true;
               color_picker.ShowHelp = false;
               DialogResult res = color_picker.ShowDialog();
               
               if(res == System.Windows.Forms.DialogResult.OK){
                    System.Drawing.Color new_rect_color = color_picker.Color;
                    rect_color = Color.FromRgb(new_rect_color.R, new_rect_color.G, new_rect_color.B);
               }


          }

          private void Help_Information(object sender, RoutedEventArgs e)
          {
               System.Windows.MessageBox.Show("- Hold left click and drag to draw a rectangle" + "\n\n" + "- Double left click on a rectangle and drag to move it around" + "\n\n" + "- To change the color of existing rectangles, select a color from the picker and triple left click a rectangle" + "\n\n" + "- Right click on a rectangle to delete it", "Controls", MessageBoxButton.OK, MessageBoxImage.Information);
          }
     }
}
