#import "EditBox_iOS.h"
#import "PlaceholderTextView.h"
#import <MobileCoreServices/UTCoreTypes.h>

/// UnityEditBox Plugin
/// Written by bkmin 2014/11 Nureka Inc.

//"Send" and "Go" return button support
//move the whole view on keyboard showing/hiding
//text alignment setting for TextView/TextField
//call onTextEditEnd on keyboard hiding
//fix bug "cannot use multi-touch" 
//UITapGestureRecognizer fixed for stripe zone on the left cannot respond on iPhone with 3DTouch

UIViewController* unityViewController = nil;
NSMutableDictionary* editBoxDict = nil;
UITapGestureRecognizer* tapRecognizer = nil;

char g_unityName[64];

bool approxEqualFloat(float x, float y)
{
    return fabs(x-y) < 0.001f;
}

@implementation EditBox

+(void) initializeEditBox:(UIViewController*) _unityViewController  unityName:(const char*) unityName
{
    unityViewController = _unityViewController;
    editBoxDict = [[NSMutableDictionary alloc] init];
    strcpy(g_unityName, unityName);
}

+(JsonObject*) makeJsonRet:(BOOL) isError error:(NSString*) strError
{
    JsonObject* jsonRet = [[JsonObject alloc] init];
    
    [jsonRet setBool:@"bError" value:isError];
    [jsonRet setString:@"strError" value:strError];
    return jsonRet;
}

+(JsonObject*) processRecvJsonMsg:(int)nSenderId msg:(JsonObject*) jsonMsg
{
    JsonObject* jsonRet;
    
    NSString* msg = [jsonMsg getString:@"msg"];
    if ([msg isEqualToString:MSG_CREATE])
    {
        EditBox* eb = [[EditBox alloc] initWithViewController:unityViewController _tag:nSenderId];
        [eb create:jsonMsg];
        [editBoxDict setObject:eb forKey:[NSNumber numberWithInt:nSenderId]];
        jsonRet = [EditBox makeJsonRet:NO error:@""];
    }
    else
    {
        EditBox* eb = [editBoxDict objectForKey:[NSNumber numberWithInt:nSenderId]];
        if (eb)
        {
            jsonRet = [eb processJsonMsg:msg json:jsonMsg];
        }
        else
        {
            jsonRet = [EditBox makeJsonRet:YES error:@"EditBox not found"];
        }
    }
    return jsonRet;
}

+(void) finalizeEditBox
{
    NSArray* objs = [editBoxDict allValues];
    for (EditBox* eb in objs)
    {
        [eb remove];
    }
    editBoxDict = nil;
}

-(void) sendJsonToUnity:(JsonObject*) json
{
    [json setInt:@"senderId" value:tag];
    
    
    NSData *jsonData = [json serialize];
    NSString *jsonString = [[NSString alloc] initWithData:jsonData encoding:NSUTF8StringEncoding];
    UnitySendMessage(g_unityName, "OnMsgFromPlugin", [jsonString UTF8String]);
}

-(JsonObject*) processJsonMsg:(NSString*) msg json:(JsonObject*) jsonMsg
{
    JsonObject* jsonRet = [EditBox makeJsonRet:NO error:@""];
    if ([msg isEqualToString:MSG_REMOVE])
    {
        [self remove];
    }
    else if ([msg isEqualToString:MSG_SET_TEXT])
    {
        [self setText:jsonMsg];
    }
    else if ([msg isEqualToString:MSG_GET_TEXT])
    {
        NSString* text = [self getText];
        [jsonRet setString:@"text" value:text];
    }
    else if ([msg isEqualToString:MSG_SET_RECT])
    {
        [self setRect:jsonMsg];
    }
    else if ([msg isEqualToString:MSG_SET_TEXTSIZE])
    {
        [self setTextSize:jsonMsg];
    }
    else if ([msg isEqualToString:MSG_SET_FOCUS])
    {
        BOOL isFocus = [jsonMsg getBool:@"isFocus"];
        [self setFocus:isFocus];
    }
    else if ([msg isEqualToString:MSG_SET_VISIBLE])
    {
        BOOL isVisible = [jsonMsg getBool:@"isVisible"];
        [self setVisible:isVisible];
    }
    
    return jsonRet;
}

-(id)initWithViewController:(UIViewController*)theViewController _tag:(int)_tag
{
    if(self = [super init]) {
        viewController = theViewController;
        tag = _tag;
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillShow:) name:UIKeyboardWillShowNotification object:nil];
        [[NSNotificationCenter defaultCenter] addObserver:self selector:@selector(keyboardWillHide:) name:UIKeyboardWillHideNotification object:nil];
    }
    return self;
}

-(void) setRect:(JsonObject*)json
{
    float x = [json getFloat:@"x"] * viewController.view.bounds.size.width;
    float y = [json getFloat:@"y"] * viewController.view.bounds.size.height;
    float width = [json getFloat:@"width"] * viewController.view.bounds.size.width;
    float height = [json getFloat:@"height"] * viewController.view.bounds.size.height;
    
    //x -= editView.superview.frame.origin.x;
    //y -= editView.superview.frame.origin.y;
    editView.frame = CGRectMake(x, y, width, height);
}

-(void) setTextSize:(JsonObject*)json
{
    float fontSize = [json getFloat:@"fontSize"];
    // Conversion for retina displays
    fontSize = fontSize / [UIScreen mainScreen].scale;

    if([editView isKindOfClass:[UITextField class]])
    {
        UITextField *textField = ((UITextField*)editView);
        UIFont *newFont = [[textField font] fontWithSize:fontSize];
        [textField setFont:newFont];
    }
    else if([editView isKindOfClass:[UITextView class]])
    {
        UITextView *textView = ((UITextView*)editView);
        UIFont *newFont = [[textView font] fontWithSize:fontSize];
        [textView setFont:newFont];
    }
}

-(void) create:(JsonObject*)json
{
    NSString* placeholder = [json getString:@"placeHolder"];
    
    NSString* font = [json getString:@"font"];
    float fontSize = [json getFloat:@"fontSize"];
    
    float x = [json getFloat:@"x"] * viewController.view.bounds.size.width;
    float y = [json getFloat:@"y"] * viewController.view.bounds.size.height;
    float width = [json getFloat:@"width"] * viewController.view.bounds.size.width;
    float height = [json getFloat:@"height"] * viewController.view.bounds.size.height;
    
    [self setMaxLength:[json getInt:@"characterLimit"]];
    
    float textColor_r = [json getFloat:@"textColor_r"];
    float textColor_g = [json getFloat:@"textColor_g"];
    float textColor_b = [json getFloat:@"textColor_b"];
    float textColor_a = [json getFloat:@"textColor_a"];
    UIColor* textColor = [UIColor colorWithRed:textColor_r green:textColor_g blue:textColor_b alpha:textColor_a];
    
    float backColor_r = [json getFloat:@"backColor_r"];
    float backColor_g = [json getFloat:@"backColor_g"];
    float backColor_b = [json getFloat:@"backColor_b"];
    float backColor_a = [json getFloat:@"backColor_a"];
    UIColor* backgroundColor = [UIColor colorWithRed:backColor_r green:backColor_g blue:backColor_b alpha:backColor_a];
    
    float placeHolderColor_r = [json getFloat:@"placeHolderColor_r"];
    float placeHolderColor_g = [json getFloat:@"placeHolderColor_g"];
    float placeHolderColor_b = [json getFloat:@"placeHolderColor_b"];
    float placeHolderColor_a = [json getFloat:@"placeHolderColor_a"];
    UIColor* placeHolderColor = [UIColor colorWithRed:placeHolderColor_r green:placeHolderColor_g blue:placeHolderColor_b alpha:placeHolderColor_a];
    
    NSString* contentType = [json getString:@"contentType"];
    NSString* alignment = [json getString:@"align"];
    BOOL withDoneButton = [json getBool:@"withDoneButton"];
    BOOL multiline = [json getBool:@"multiline"];
    
    BOOL autoCorr = NO;
    BOOL password = NO;
    UIKeyboardType keyType = UIKeyboardTypeDefault;
    
    if ([contentType isEqualToString:@"Autocorrected"])
    {
        autoCorr = YES;
    }
    else if ([contentType isEqualToString:@"IntegerNumber"])
    {
        keyType = UIKeyboardTypeNumberPad;
    }
    else if ([contentType isEqualToString:@"DecimalNumber"])
    {
        keyType = UIKeyboardTypeDecimalPad;
    }
    else if ([contentType isEqualToString:@"Alphanumeric"])
    {
        keyType = UIKeyboardTypeAlphabet;
    }
    else if ([contentType isEqualToString:@"Name"])
    {
        keyType = UIKeyboardTypeNamePhonePad;
    }
    else if ([contentType isEqualToString:@"EmailAddress"])
    {
        keyType = UIKeyboardTypeEmailAddress;
    }
    else if ([contentType isEqualToString:@"Password"])
    {
        password = YES;
    }
    else if ([contentType isEqualToString:@"Pin"])
    {
        keyType = UIKeyboardTypePhonePad;
    }
    
    NSTextAlignment textAlignment;
    if ([alignment isEqualToString:@"UpperLeft"])
    {
        textAlignment = NSTextAlignmentLeft;
    }
    else if ([alignment isEqualToString:@"UpperCenter"])
    {
        textAlignment = NSTextAlignmentCenter;
    }
    else if ([alignment isEqualToString:@"UpperRight"])
    {
        textAlignment = NSTextAlignmentRight;
    }
    else if ([alignment isEqualToString:@"MiddleLeft"])
    {
        textAlignment = NSTextAlignmentLeft;
    }
    else if ([alignment isEqualToString:@"MiddleCenter"])
    {
        textAlignment = NSTextAlignmentCenter;
    }
    else if ([alignment isEqualToString:@"MiddleRight"])
    {
        textAlignment = NSTextAlignmentRight;
    }
    else if ([alignment isEqualToString:@"LowerLeft"])
    {
        textAlignment = NSTextAlignmentLeft;
    }
    else if ([alignment isEqualToString:@"LowerCenter"])
    {
        textAlignment = NSTextAlignmentCenter;
    }
    else if ([alignment isEqualToString:@"LowerRight"])
    {
        textAlignment = NSTextAlignmentRight;
    }
   
    if (withDoneButton)
    {
        keyboardDoneButtonView  = [[UIToolbar alloc] init];
        [keyboardDoneButtonView sizeToFit];
        doneButton = [[UIBarButtonItem alloc] initWithTitle:@"Done" style:UIBarButtonItemStyleDone target:self
                                        action:@selector(doneClicked:)];
        
        UIBarButtonItem *flexibleSpace = [[UIBarButtonItem alloc] initWithBarButtonSystemItem:UIBarButtonSystemItemFlexibleSpace target:nil action:nil];
        [keyboardDoneButtonView setItems:[NSArray arrayWithObjects:flexibleSpace, flexibleSpace,doneButton, nil]];
    }
    else
    {
        keyboardDoneButtonView = nil;
    }
    
    UIReturnKeyType returnKeyType = UIReturnKeyDefault;
    NSString* returnKeyTypeString = [json getString:@"return_key_type"];
    if ([returnKeyTypeString isEqualToString:@"Next"])
    {
        returnKeyType = UIReturnKeyNext;
    }
    else if ([returnKeyTypeString isEqualToString:@"Done"])
    {
        returnKeyType = UIReturnKeyDone;
    }
    else if ([returnKeyTypeString isEqualToString:@"Send"])
    {
        returnKeyType = UIReturnKeySend;
    }
    else if ([returnKeyTypeString isEqualToString:@"Go"])
    {
        returnKeyType = UIReturnKeyGo;
    }
    
    // Conversion for retina displays
    fontSize = fontSize / [UIScreen mainScreen].scale;
    
    UIFont* uiFont;
    if ([font length] > 0)
    {
        uiFont = [UIFont fontWithName:font size:fontSize];
    }
    else
    {
        uiFont = [UIFont systemFontOfSize:fontSize];
    }    
    
    if (multiline)
    {
        PlaceholderTextView* textView = [[PlaceholderTextView alloc] initWithFrame:CGRectMake(x, y, width, height)];
        textView.keyboardType = keyType;
        
        [textView setFont:uiFont];
        
        textView.scrollEnabled = TRUE;
        
        textView.delegate = self;
        textView.tag = 0;
        textView.text = @"";
        
        textView.textColor = textColor;
        textView.backgroundColor = backgroundColor;
        textView.returnKeyType = returnKeyType;
        textView.textAlignment = textAlignment;
        textView.autocorrectionType = autoCorr ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo;
        textView.contentInset = UIEdgeInsetsMake(0.0f, 0.0f, 0.0f, 0.0f);
        textView.placeholder = placeholder;
        textView.placeholderColor = placeHolderColor;
        textView.delegate = self;
        if (keyType == UIKeyboardTypeEmailAddress)
            textView.autocapitalizationType = UITextAutocapitalizationTypeNone;
        
        [textView setSecureTextEntry:password];
        if (keyboardDoneButtonView != nil) textView.inputAccessoryView = keyboardDoneButtonView;
        
        editView = textView;
    }
    else
    {
        UITextField* textField = [[UITextField alloc] initWithFrame:CGRectMake(x, y, width, height)];
        textField.keyboardType = keyType;
        [textField setFont:uiFont];
        textField.delegate = self;
        textField.tag = 0;
        textField.text = @"";
        textField.textColor = textColor;
        textField.backgroundColor = backgroundColor;
        textField.returnKeyType = returnKeyType;
        textField.autocorrectionType = autoCorr ? UITextAutocorrectionTypeYes : UITextAutocorrectionTypeNo;
        textField.textAlignment = textAlignment;
        // Settings the placeholder like this is needed because otherwise it will not be visible
        textField.attributedPlaceholder = [[NSAttributedString alloc] initWithString:placeholder attributes:@{NSForegroundColorAttributeName: placeHolderColor}];
        textField.delegate = self;
        if (keyType == UIKeyboardTypeEmailAddress)
            textField.autocapitalizationType = UITextAutocapitalizationTypeNone;
        
        [textField addTarget:self action:@selector(textFieldDidChange:) forControlEvents:UIControlEventEditingChanged];
        [textField setSecureTextEntry:password];
        if (keyboardDoneButtonView != nil) textField.inputAccessoryView = keyboardDoneButtonView;
        
        editView = textField;
    }
    [unityViewController.view addSubview:editView];
}

-(void) setText:(JsonObject*)json
{
    NSString* newText = [json getString:@"text"];
    if([editView isKindOfClass:[UITextField class]]) {
        [((UITextField*)editView) setText:newText];
    } else if([editView isKindOfClass:[UITextView class]]){
        [((UITextView*)editView) setText:newText];
    }
}

-(IBAction) doneClicked:(id)sender
{
    [self hideKeyboard];
}

-(int) getLineCount
{
    if([editView isKindOfClass:[UITextField class]]) {
        return 1;
    } else if([editView isKindOfClass:[UITextView class]]){
        UITextView* tv = ((UITextView*)editView);
        int lineCount = (int) tv.contentSize.height / tv.font.lineHeight;
        return (lineCount);
    }
    return 0;
}


-(void) remove
{
    [[NSNotificationCenter defaultCenter] removeObserver:self];
    [editView resignFirstResponder];
    [editView removeFromSuperview];
    if (keyboardDoneButtonView != nil)
    {
        doneButton = nil;
        keyboardDoneButtonView = nil;
    }
}

-(void) setFocus:(BOOL) isFocus
{
    if (isFocus)
    {
        [editView becomeFirstResponder];
    }
    else
    {
        [editView resignFirstResponder];
    }
}

-(NSString*) getText
{
    if([editView isKindOfClass:[UITextField class]]) {
        return ((UITextField*)editView).text;
    } else if([editView isKindOfClass:[UITextView class]]){
        return ((UITextView*)editView).text;
    }
    return @"";
}

-(void) hideKeyboard
{
    [editView resignFirstResponder];
}

-(void) setVisible:(bool)isVisible
{
    editView.hidden = !isVisible;
}

-(void) onTextChange:(NSString*) text
{
    JsonObject* jsonToUnity = [[JsonObject alloc] init];
    
    [jsonToUnity setString:@"msg" value:MSG_TEXT_CHANGE];
    [jsonToUnity setString:@"text" value:text];
    [self sendJsonToUnity:jsonToUnity];
}

-(void) onTextEditBegin
{
    JsonObject* jsonToUnity = [[JsonObject alloc] init];

    [jsonToUnity setString:@"msg" value:MSG_TEXT_BEGIN_EDIT];
    [self sendJsonToUnity:jsonToUnity];
}

-(void) onTextEditEnd:(NSString*) text
{
    JsonObject* jsonToUnity = [[JsonObject alloc] init];
    
    [jsonToUnity setString:@"msg" value:MSG_TEXT_END_EDIT];
    [jsonToUnity setString:@"text" value:text];
    [self sendJsonToUnity:jsonToUnity];
}

-(void) textViewDidBeginEditing:(UITextView *)textView
{
    [self onTextEditBegin];
}

-(void) textViewDidEndEditing:(UITextView *)textView
{
    [self onTextEditEnd:textView.text];
}

-(void) textViewDidChange:(UITextView *)textView
{
    [self onTextChange:textView.text];
}

-(void) textFieldDidBeginEditing:(UITextField *)textField
{
    [self onTextEditBegin];
}

-(void) textFieldDidEndEditing:(UITextField *)textField
{
    [self onTextEditEnd:textField.text];
}

- (BOOL)textFieldShouldReturn:(UITextField *)textField
{
    if (![editView isFirstResponder]) return YES;
    JsonObject* jsonToUnity = [[JsonObject alloc] init];
    
    [jsonToUnity setString:@"msg" value:MSG_RETURN_PRESSED];
    [self sendJsonToUnity:jsonToUnity];
    return YES;
}

- (BOOL)textField:(UITextField *)textField shouldChangeCharactersInRange:(NSRange)range replacementString:(NSString *)string {
    // Prevent crashing undo bug – see note below.
    if(range.length + range.location > textField.text.length)
    {
        return NO;
    }
    
    NSUInteger newLength = [textField.text length] + [string length] - range.length;
    if([self maxLength] > 0)
        return newLength <= [self maxLength];
    else
        return YES;
}

-(void) textFieldDidChange :(UITextField *)theTextField{
    [self onTextChange:theTextField.text];
}

-(void) keyboardWillShow:(NSNotification *)notification
{
    if (![editView isFirstResponder]) return;
    
    //get keyboard height
    CGFloat kbHeight = [[notification.userInfo objectForKey:UIKeyboardFrameEndUserInfoKey] CGRectValue].size.height;
    //calculate the distance from the top of keyboard to the bottom of TextView/TextField
    CGFloat offset = (editView.frame.origin.y + editView.frame.size.height) - (unityViewController.view.frame.size.height - kbHeight);
    //get the duration of keyboard showing animation
    double duration = [[notification.userInfo objectForKey:UIKeyboardAnimationDurationUserInfoKey] doubleValue];
    //move the whole view
    if (offset > 0){
        [UIView animateWithDuration:duration animations:^{
            unityViewController.view.frame = CGRectMake(0, -offset, unityViewController.view.frame.size.width, unityViewController.view.frame.size.height);
        }];
    }

    //add tapRecognizer on keyboard showing
    if (tapRecognizer == nil){
        tapRecognizer = [[UITapGestureRecognizer alloc] initWithTarget:self action:@selector(tapAction:)];
        [unityViewController.view addGestureRecognizer:tapRecognizer];
    }
}

-(void) keyboardWillHide:(NSNotification*)notification
{
    if (![editView isFirstResponder]) return;

    //get the duration of keyboard hiding animation
    double duration = [[notification.userInfo objectForKey:UIKeyboardAnimationDurationUserInfoKey] doubleValue];
    //move the whole view
    [UIView animateWithDuration:duration animations:^{
        unityViewController.view.frame = CGRectMake(0, 0, unityViewController.view.frame.size.width, unityViewController.view.frame.size.height);
    }];

    //remove tapRecognizer on keyboard hiding
    if (tapRecognizer != nil){
        [unityViewController.view removeGestureRecognizer:tapRecognizer];
        tapRecognizer = nil;
    }

    //call onTextEditEnd
    if ([editView isKindOfClass:[UITextView class]]){
        UITextView* textView = (UITextView*) editView;
        [self onTextEditEnd:textView.text];
    } else if ([editView isKindOfClass:[UITextField class]]){
        UITextField* textField = (UITextField*) editView;
        [self onTextEditEnd:textField.text];
    }
}

-(BOOL) isFocused
{
    return editView.isFirstResponder;
}

-(void) tapAction:(id) sender
{
    for (EditBox *eb in [editBoxDict allValues])
    {
        if ([eb isFocused])
        {
            [eb hideKeyboard];
        }
    }
}

@end
