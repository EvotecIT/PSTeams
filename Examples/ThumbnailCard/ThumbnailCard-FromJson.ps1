$Wrapper = @"
{
    "contentType": "application/vnd.microsoft.card.thumbnail",
    "content": {
      "title": "Bender",
      "subtitle": "tale of a robot who dared to love",
      "text": "Bender Bending Rodríguez is a main character in the animated television series Futurama. He was created by series creators Matt Groening and David X. Cohen, and is voiced by John DiMaggio",
      "images": [
        {
          "url": "https://upload.wikimedia.org/wikipedia/en/a/a6/Bender_Rodriguez.png",
          "alt": "Bender Rodríguez"
        }
      ],
      "buttons": [
        {
          "type": "imBack",
          "title": "Thumbs Up",
          "image": "http://moopz.com/assets_c/2012/06/emoji-thumbs-up-150-thumb-autox125-140616.jpg",
          "value": "I like it"
        },
        {
          "type": "imBack",
          "title": "Thumbs Down",
          "image": "http://yourfaceisstupid.com/wp-content/uploads/2014/08/thumbs-down.png",
          "value": "I don't like it"
        },
        {
          "type": "openUrl",
          "title": "I feel lucky",
          "image": "http://thumb9.shutterstock.com/photos/thumb_large/683806/148441982.jpg",
          "value": "https://www.bing.com/images/search?q=bender&qpvt=bender&qpvt=bender&qpvt=bender&FORM=IGRE"
        }
      ],
      "tap": {
        "type": "imBack",
        "value": "Tapped it!"
      }
    }
  }
"@

Send-TeamsMessageBody -Uri $Env:TEAMSPESTERID -Body $Wrapper -Wrap